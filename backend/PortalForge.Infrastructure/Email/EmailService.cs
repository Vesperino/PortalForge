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
