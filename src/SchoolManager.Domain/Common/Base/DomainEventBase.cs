using SchoolManager.Domain.Common.Interfaces;

namespace SchoolManager.Domain.Common.Base;

/// <summary>
/// Base para todos os Domain Events.
/// Use record para imutabilidade e igualdade estrutural automática.
/// </summary>
public abstract record DomainEventBase : IDomainEvent
{
    public DateTime DateOccurred { get; protected init; } = DateTime.UtcNow;
}
