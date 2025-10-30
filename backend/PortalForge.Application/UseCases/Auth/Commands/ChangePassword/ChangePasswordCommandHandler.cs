using MediatR;
using PortalForge.Application.Common.Interfaces;

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
            throw new Exception("User not found");
        }

        // Verify current password by attempting to sign in
        var loginResult = await _authService.SignInAsync(user.Email, request.CurrentPassword);
        if (loginResult.AccessToken == null || loginResult.RefreshToken == null)
        {
            throw new Exception("Current password is incorrect");
        }

        // Change password in Supabase
        var changeResult = await _authService.UpdatePasswordAsync(
            loginResult.AccessToken,
            loginResult.RefreshToken,
            request.NewPassword);
        if (!changeResult)
        {
            throw new Exception("Failed to change password");
        }

        // Update MustChangePassword flag in database
        user.MustChangePassword = false;
        await _unitOfWork.UserRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}

