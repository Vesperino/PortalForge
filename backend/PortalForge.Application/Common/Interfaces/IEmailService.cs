namespace PortalForge.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string toEmail, string verificationLink);
    Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
    Task SendPasswordChangedEmailAsync(string toEmail);
}
