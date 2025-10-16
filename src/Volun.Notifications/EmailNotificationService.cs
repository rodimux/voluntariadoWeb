using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using Volun.Core.Services;

namespace Volun.Notifications;

public class EmailNotificationService : INotificationService
{
    private readonly SmtpOptions _options;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(IOptions<SmtpOptions> options, ILogger<EmailNotificationService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Notificaciones SMTP deshabilitadas. Mensaje no enviado. Destinatario: {To}", message.To);
            return;
        }

        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
        email.To.Add(MailboxAddress.Parse(message.To));
        if (!string.IsNullOrWhiteSpace(message.Cc))
        {
            email.Cc.Add(MailboxAddress.Parse(message.Cc));
        }
        if (!string.IsNullOrWhiteSpace(message.Bcc))
        {
            email.Bcc.Add(MailboxAddress.Parse(message.Bcc));
        }
        email.Subject = message.Subject;
        email.Body = new TextPart(TextFormat.Html)
        {
            Text = $"<!-- Template: {message.TemplateKey} -->\n<p>Contenido pendiente de plantilla.</p>"
        };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_options.Host, _options.Port, SecureSocketOptions.StartTlsWhenAvailable, cancellationToken);

        if (!string.IsNullOrWhiteSpace(_options.UserName))
        {
            await smtp.AuthenticateAsync(_options.UserName, _options.Password, cancellationToken);
        }

        await smtp.SendAsync(email, cancellationToken);
        await smtp.DisconnectAsync(true, cancellationToken);
    }
}

public record SmtpOptions
{
    public bool Enabled { get; init; } = false;
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 25;
    public string FromEmail { get; init; } = "no-reply@volun.local";
    public string FromName { get; init; } = "Volun";
    public string? UserName { get; init; }
    public string? Password { get; init; }
};
