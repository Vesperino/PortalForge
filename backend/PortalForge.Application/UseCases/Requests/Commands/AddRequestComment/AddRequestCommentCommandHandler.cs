using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Application.Interfaces;
using PortalForge.Application.Services;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.UseCases.Requests.Commands.AddRequestComment;

/// <summary>
/// Handler for AddRequestCommentCommand.
/// Adds a comment to a request and notifies relevant parties.
/// </summary>
public class AddRequestCommentCommandHandler : IRequestHandler<AddRequestCommentCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AddRequestCommentCommandHandler> _logger;

    public AddRequestCommentCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IUnifiedValidatorService validatorService,
        ICurrentUserService currentUserService,
        ILogger<AddRequestCommentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _validatorService = validatorService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Guid> Handle(AddRequestCommentCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate
        await _validatorService.ValidateAsync(request);

        // 2. Get request with related entities
        var existingRequest = await _unitOfWork.RequestRepository.GetByIdAsync(request.RequestId)
            ?? throw new NotFoundException($"Request with ID {request.RequestId} not found");

        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId)
            ?? throw new NotFoundException($"User with ID {request.UserId} not found");

        // 3. Authorization check - submitter, approvers, Admin and HR can comment
        var isSubmitter = existingRequest.SubmittedById == request.UserId;
        var isApprover = existingRequest.ApprovalSteps
            .Any(s => s.ApproverId == request.UserId);
        var isAdmin = _currentUserService.IsInRole("Admin");
        var isHR = _currentUserService.IsInRole("HR");

        if (!isSubmitter && !isApprover && !isAdmin && !isHR)
        {
            throw new ForbiddenException("Możesz komentować tylko wnioski, w których jesteś składającym lub zatwierdzającym");
        }

        // 4. Create comment
        var comment = new RequestComment
        {
            Id = Guid.NewGuid(),
            RequestId = request.RequestId,
            UserId = request.UserId,
            Comment = request.Comment,
            Attachments = request.Attachments,
            CreatedAt = DateTime.UtcNow
        };

        var commentId = await _unitOfWork.RequestCommentRepository.CreateAsync(comment);

        // 5. Notify relevant parties
        // Notify submitter (if commenter is not submitter)
        if (!isSubmitter)
        {
            await _notificationService.CreateNotificationAsync(
                userId: existingRequest.SubmittedById,
                type: NotificationType.RequestCommented,
                title: "Nowy komentarz do wniosku",
                message: $"{user.FirstName} {user.LastName} dodał/a komentarz do Twojego wniosku {existingRequest.RequestNumber}",
                relatedEntityType: "Request",
                relatedEntityId: existingRequest.Id.ToString(),
                actionUrl: $"/dashboard/requests/{existingRequest.Id}");
        }

        // Notify all approvers (except commenter)
        var approversToNotify = existingRequest.ApprovalSteps
            .Where(s => s.ApproverId != request.UserId)
            .Select(s => s.ApproverId)
            .Distinct();

        foreach (var approverId in approversToNotify)
        {
            await _notificationService.CreateNotificationAsync(
                userId: approverId,
                type: NotificationType.RequestCommented,
                title: "Nowy komentarz do wniosku",
                message: $"{user.FirstName} {user.LastName} dodał/a komentarz do wniosku {existingRequest.RequestNumber}",
                relatedEntityType: "Request",
                relatedEntityId: existingRequest.Id.ToString(),
                actionUrl: $"/dashboard/requests/{existingRequest.Id}");
        }

        _logger.LogInformation(
            "Comment added to request {RequestId} by user {UserId}",
            existingRequest.Id, request.UserId);

        return commentId;
    }
}
