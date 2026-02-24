namespace SchoolManager.Domain.Common.Interfaces;

/// <summary>
/// Contrato para handlers de Domain Events.
/// Implementações ficam em Infrastructure e são registradas no DI.
/// O EscolaDbContext despacha os eventos após SaveChangesAsync.
/// </summary>
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
