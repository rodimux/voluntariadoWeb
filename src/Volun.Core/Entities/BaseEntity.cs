namespace Volun.Core.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; protected init; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; protected set; }

    protected void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
