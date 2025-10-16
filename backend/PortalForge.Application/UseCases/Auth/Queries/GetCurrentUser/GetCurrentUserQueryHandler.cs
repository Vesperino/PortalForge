using System.Net;
using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.UseCases.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, User>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetCurrentUserQueryHandler> _logger;

    public GetCurrentUserQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetCurrentUserQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<User> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == null)
        {
            _logger.LogWarning("No user ID provided");
            throw new CustomException("Użytkownik nie jest zalogowany", null, HttpStatusCode.Unauthorized);
        }

        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId.Value);

        if (user == null)
        {
            _logger.LogWarning("User not found in database: {UserId}", request.UserId);
            throw new NotFoundException($"Użytkownik o ID {request.UserId} nie został znaleziony");
        }

        return user;
    }
}
