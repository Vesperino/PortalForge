using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.DTOs;
using PortalForge.Application.Services;

namespace PortalForge.Application.UseCases.Admin.Queries.GetAuditLogs;

/// <summary>
/// Handler for GetAuditLogsQuery.
/// Retrieves audit logs with filtering and pagination.
/// </summary>
public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, PagedResult<AuditLogDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly ILogger<GetAuditLogsQueryHandler> _logger;

    public GetAuditLogsQueryHandler(
        IUnitOfWork unitOfWork,
        IUnifiedValidatorService validatorService,
        ILogger<GetAuditLogsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _validatorService = validatorService;
        _logger = logger;
    }

    public async Task<PagedResult<AuditLogDto>> Handle(
        GetAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Validate
        await _validatorService.ValidateAsync(request);

        // 2. Get filtered and paginated audit logs
        var (totalCount, items) = await _unitOfWork.AuditLogRepository.GetFilteredAsync(
            request.EntityType,
            request.Action,
            request.UserId,
            request.FromDate,
            request.ToDate,
            request.Page,
            request.PageSize);

        // 3. Map to DTOs
        var dtos = items.Select(al => new AuditLogDto
        {
            Id = al.Id,
            EntityType = al.EntityType,
            EntityId = al.EntityId,
            Action = al.Action,
            UserId = al.UserId,
            UserFullName = al.User != null ? $"{al.User.FirstName} {al.User.LastName}" : null,
            OldValue = al.OldValue,
            NewValue = al.NewValue,
            Reason = al.Reason,
            Timestamp = al.Timestamp,
            IpAddress = al.IpAddress
        }).ToList();

        // 4. Build paged result
        var result = new PagedResult<AuditLogDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };

        _logger.LogInformation(
            "Retrieved {Count} audit logs (page {Page}/{TotalPages}, total: {TotalCount})",
            result.Items.Count, result.Page, result.TotalPages, result.TotalCount);

        return result;
    }
}
