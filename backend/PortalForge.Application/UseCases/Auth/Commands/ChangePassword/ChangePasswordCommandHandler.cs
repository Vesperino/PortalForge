using MediatR;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;

namespace PortalForge.Application.UseCases.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
{
    private readonly ISupabaseAuthService _authService;
    private readonly IUnitOfWork _unitOfWork;

    public ChangePasswordCommandHandler(
        ISupabaseAuthService authService,
        IUnitOfWork unitOfWork)
    {
        _authService = authService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        // Get user from database
        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        // Verify current password by attempting to sign in
        var loginResult = await _authService.SignInAsync(user.Email, request.CurrentPassword);
        if (loginResult.AccessToken == null || loginResult.RefreshToken == null)
        {
            throw new ValidationException("Current password is incorrect");
        }

        // Change password in Supabase
        var changeResult = await _authService.UpdatePasswordAsync(
            loginResult.AccessToken,
            loginResult.RefreshToken,
            request.NewPassword);
        if (!changeResult)
        {
            throw new BusinessException("Failed to change password. Please try again later.");
        }

        // Update MustChangePassword flag in database
        user.MustChangePassword = false;
        await _unitOfWork.UserRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}

