using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Infrastructure.Email.Models;
using System.Reflection;

namespace PortalForge.Infrastructure.Email;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> emailSettings,
        ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendVerificationEmailAsync(string toEmail, string verificationLink)
    {
        var template = GetEmailTemplate("VerificationEmail.html");
        var body = template.Replace("{{VERIFICATION_LINK}}", verificationLink);

        var message = new EmailMessage
        {
            To = toEmail,
            Subject = "Aktywuj swoje konto w PortalForge",
            Body = body,
            IsHtml = true
        };

        await SendEmailAsync(message);
        _logger.LogInformation("Verification email sent to {Email}", toEmail);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        var template = GetEmailTemplate("PasswordResetEmail.html");
        var body = template.Replace("{{RESET_LINK}}", resetLink);

        var message = new EmailMessage
        {
            To = toEmail,
            Subject = "Resetowanie hasła w PortalForge",
            Body = body,
            IsHtml = true
        };

        await SendEmailAsync(message);
        _logger.LogInformation("Password reset email sent to {Email}", toEmail);
    }

    public async Task SendPasswordChangedEmailAsync(string toEmail)
    {
        var template = GetEmailTemplate("PasswordChangedEmail.html");
        var body = template.Replace("{{DATE_TIME}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        var message = new EmailMessage
        {
            To = toEmail,
            Subject = "Hasło zostało zmienione - PortalForge",
            Body = body,
            IsHtml = true
        };

        await SendEmailAsync(message);
        _logger.LogInformation("Password changed notification sent to {Email}", toEmail);
    }

    public async Task SendNotificationEmailAsync(
        string toEmail,
        string toName,
        string notificationTitle,
        string notificationMessage,
        string? actionUrl = null,
        string? actionButtonText = null)
    {
        if (!_emailSettings.Enabled)
        {
            _logger.LogWarning("Email sending is disabled. Skipping notification email to {Email}", toEmail);
            return;
        }

        var htmlBody = BuildNotificationEmailTemplate(
            toName,
            notificationTitle,
            notificationMessage,
            actionUrl,
            actionButtonText);

        var message = new EmailMessage
        {
            To = toEmail,
            Subject = $"PortalForge - {notificationTitle}",
            Body = htmlBody,
            IsHtml = true
        };

        await SendEmailAsync(message);
        _logger.LogInformation("Notification email sent to {Email}: {Title}", toEmail, notificationTitle);
    }

    private string BuildNotificationEmailTemplate(
        string recipientName,
        string title,
        string message,
        string? actionUrl,
        string? actionButtonText)
    {
        var actionButton = string.Empty;
        if (!string.IsNullOrEmpty(actionUrl) && !string.IsNullOrEmpty(actionButtonText))
        {
            actionButton = $@"
                <div style=""margin: 30px 0; text-align: center;"">
                    <a href=""{actionUrl}""
                       style=""background-color: #2563eb; color: white; padding: 12px 24px;
                              text-decoration: none; border-radius: 6px; display: inline-block;
                              font-weight: 500;"">
                        {actionButtonText}
                    </a>
                </div>";
        }

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f3f4f6;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f3f4f6; padding: 20px;"">
        <tr>
            <td align=""center"">
                <table width=""600"" cellpadding=""0"" cellspacing=""0""
                       style=""background-color: white; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); max-width: 600px;"">
                    <!-- Header -->
                    <tr>
                        <td style=""background-color: #2563eb; padding: 30px; text-align: center; border-radius: 8px 8px 0 0;"">
                            <h1 style=""color: white; margin: 0; font-size: 24px; font-weight: 600;"">PortalForge</h1>
                        </td>
                    </tr>

                    <!-- Body -->
                    <tr>
                        <td style=""padding: 40px 30px;"">
                            <p style=""margin: 0 0 20px 0; color: #374151; font-size: 16px;"">
                                Witaj {recipientName},
                            </p>

                            <h2 style=""color: #111827; margin: 0 0 15px 0; font-size: 20px; font-weight: 600;"">
                                {title}
                            </h2>

                            <p style=""color: #4b5563; line-height: 1.6; margin: 0 0 20px 0; white-space: pre-line;"">
                                {message}
                            </p>

                            {actionButton}
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style=""background-color: #f9fafb; padding: 20px 30px; text-align: center;
                                   border-radius: 0 0 8px 8px; border-top: 1px solid #e5e7eb;"">
                            <p style=""color: #6b7280; font-size: 14px; margin: 0; line-height: 1.5;"">
                                To jest automatyczna wiadomość z systemu PortalForge.<br>
                                Nie odpowiadaj na ten email.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private async Task SendEmailAsync(EmailMessage emailMessage)
    {
        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
        mimeMessage.To.Add(MailboxAddress.Parse(emailMessage.To));
        mimeMessage.Subject = emailMessage.Subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = emailMessage.IsHtml ? emailMessage.Body : null,
            TextBody = !emailMessage.IsHtml ? emailMessage.Body : null
        };

        mimeMessage.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(
                _emailSettings.SmtpServer,
                _emailSettings.SmtpPort,
                _emailSettings.UseTLS ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

            await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            await client.SendAsync(mimeMessage);

            _logger.LogInformation("Email sent successfully to {To}", emailMessage.To);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", emailMessage.To);
            throw;
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }

    private string GetEmailTemplate(string templateName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"PortalForge.Infrastructure.Email.Templates.{templateName}";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException($"Email template not found: {templateName}");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
