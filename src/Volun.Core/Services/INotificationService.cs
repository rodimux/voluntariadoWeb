namespace Volun.Core.Services;

public interface INotificationService
{
    Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = default);
}

public record NotificationMessage(
    string To,
    string Subject,
    string TemplateKey,
    IDictionary<string, string> Parameters,
    string? Cc = null,
    string? Bcc = null);
