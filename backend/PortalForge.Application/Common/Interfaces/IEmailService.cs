namespace PortalForge.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string toEmail, string verificationLink);
    Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
    Task SendPasswordChangedEmailAsync(string toEmail);

    /// <summary>
    /// Sends a notification email based on a template
    /// </summary>
    Task SendNotificationEmailAsync(
        string toEmail,
        string toName,
        string notificationTitle,
        string notificationMessage,
        string? actionUrl = null,
        string? actionButtonText = null);
}
