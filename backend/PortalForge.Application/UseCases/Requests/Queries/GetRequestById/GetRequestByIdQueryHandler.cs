using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Exceptions;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.Requests.DTOs;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.UseCases.Requests.Queries.GetRequestById;

public class GetRequestByIdQueryHandler : IRequestHandler<GetRequestByIdQuery, RequestDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetRequestByIdQueryHandler> _logger;
    private readonly IConfiguration _configuration;
    private readonly ICurrentUserService _currentUserService;

    public GetRequestByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetRequestByIdQueryHandler> logger,
        IConfiguration configuration,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _configuration = configuration;
        _currentUserService = currentUserService;
    }

    public async Task<RequestDetailDto> Handle(GetRequestByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching request details for RequestId: {RequestId}", request.RequestId);

        var req = await _unitOfWork.RequestRepository.GetByIdAsync(request.RequestId);

        if (req == null)
        {
            _logger.LogWarning("Request not found: {RequestId}", request.RequestId);
            throw new NotFoundException($"Request with ID {request.RequestId} not found");
        }

        var hasAccess = await HasAccessToRequestAsync(req, request.CurrentUserId);
        if (!hasAccess)
        {
            _logger.LogWarning(
                "Access denied to request {RequestId} for user {UserId}",
                request.RequestId,
                request.CurrentUserId);
            throw new ForbiddenException("You do not have permission to view this request");
        }

        // Get comments
        var comments = await _unitOfWork.RequestCommentRepository.GetByRequestIdAsync(request.RequestId);

        // Get edit history
        var editHistory = await _unitOfWork.RequestEditHistoryRepository.GetByRequestIdAsync(request.RequestId);

        // Get template with quiz questions
        var template = await _unitOfWork.RequestTemplateRepository.GetByIdAsync(req.RequestTemplateId);

        // Map to DTO
        var dto = new RequestDetailDto
        {
            Id = req.Id,
            RequestNumber = req.RequestNumber,
            RequestTemplateId = req.RequestTemplateId,
            RequestTemplateName = req.RequestTemplate?.Name ?? string.Empty,
            RequestTemplateIcon = req.RequestTemplate?.Icon ?? string.Empty,
            SubmittedById = req.SubmittedById,
            SubmittedByName = $"{req.SubmittedBy.FirstName} {req.SubmittedBy.LastName}",
            SubmittedAt = req.SubmittedAt,
            Priority = req.Priority.ToString(),
            FormData = req.FormData,
            LeaveType = req.LeaveType?.ToString(),
            Attachments = ParseAttachments(req.Attachments),
            Status = req.Status.ToString(),
            CompletedAt = req.CompletedAt,
            ApprovalSteps = req.ApprovalSteps.Select(s =>
            {
                // Find the template for this step to get quiz questions
                var stepTemplate = template?.ApprovalStepTemplates
                    .FirstOrDefault(ast => ast.Id == s.RequestApprovalStepTemplateId);

                return new ApprovalStepDto
                {
                    Id = s.Id,
                    StepOrder = s.StepOrder,
                    ApproverId = s.ApproverId,
                    ApproverName = s.Approver != null ? $"{s.Approver.FirstName} {s.Approver.LastName}" : "Unknown",
                    Status = s.Status.ToString(),
                    Comment = s.Comment,
                    FinishedAt = s.FinishedAt,
                    RequiresQuiz = s.RequiresQuiz,
                    QuizScore = s.QuizScore,
                    QuizPassed = s.QuizPassed,
                    PassingScore = s.PassingScore ?? stepTemplate?.PassingScore,
                    QuizQuestions = stepTemplate?.QuizQuestions
                        .OrderBy(q => q.Order)
                        .Select(q => new QuizQuestionDto
                        {
                            Id = q.Id,
                            Question = q.Question,
                            Options = q.Options,
                            Order = q.Order
                        }).ToList() ?? new List<QuizQuestionDto>(),
                    QuizAnswers = s.QuizAnswers
                        .Select(a => new QuizAnswerDto
                        {
                            QuestionId = a.QuizQuestionId,
                            SelectedAnswer = a.SelectedAnswer,
                            IsCorrect = a.IsCorrect,
                            AnsweredAt = a.AnsweredAt
                        }).ToList()
                };
            }).OrderBy(s => s.StepOrder).ToList(),
            Comments = comments.Select(c => new RequestCommentDto
            {
                Id = c.Id,
                RequestId = c.RequestId,
                UserId = c.UserId,
                UserName = c.User != null ? $"{c.User.FirstName} {c.User.LastName}" : "Unknown",
                Comment = c.Comment,
                Attachments = ParseAttachments(c.Attachments),
                CreatedAt = c.CreatedAt
            }).OrderBy(c => c.CreatedAt).ToList(),
            EditHistory = editHistory.Select(h => new RequestEditHistoryDto
            {
                Id = h.Id,
                RequestId = h.RequestId,
                EditedByUserId = h.EditedByUserId,
                EditedByUserName = h.EditedBy != null ? $"{h.EditedBy.FirstName} {h.EditedBy.LastName}" : "Unknown",
                EditedAt = h.EditedAt,
                OldFormData = h.OldFormData,
                NewFormData = h.NewFormData,
                ChangeReason = h.ChangeReason
            }).OrderByDescending(h => h.EditedAt).ToList()
        };

        _logger.LogInformation(
            "Successfully fetched request details: {RequestNumber} with {CommentCount} comments and {HistoryCount} edits",
            req.RequestNumber, dto.Comments.Count, dto.EditHistory.Count);

        return dto;
    }

    private List<string> ParseAttachments(string? attachmentsJson)
    {
        if (string.IsNullOrWhiteSpace(attachmentsJson))
            return new List<string>();

        try
        {
            var relativePaths = System.Text.Json.JsonSerializer.Deserialize<List<string>>(attachmentsJson) ?? new List<string>();

            // Convert relative paths to full URLs
            var baseUrl = _configuration["AppSettings:ApiUrl"] ?? "http://localhost:5000";
            return relativePaths.Select(path => $"{baseUrl.TrimEnd('/')}/api/storage/files/{path}").ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse attachments JSON: {Json}", attachmentsJson);
            return new List<string>();
        }
    }

    private Task<bool> HasAccessToRequestAsync(Request request, Guid currentUserId)
    {
        // User can view their own requests
        if (request.SubmittedById == currentUserId)
        {
            return Task.FromResult(true);
        }

        // Approvers can view requests assigned to them
        var isApprover = request.ApprovalSteps.Any(s => s.ApproverId == currentUserId);
        if (isApprover)
        {
            return Task.FromResult(true);
        }

        // Admin and HR can view all requests
        var isAdmin = _currentUserService.IsInRole(UserRole.Admin.ToString());
        var isHR = _currentUserService.IsInRole(UserRole.HR.ToString());
        if (isAdmin || isHR)
        {
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }
}
