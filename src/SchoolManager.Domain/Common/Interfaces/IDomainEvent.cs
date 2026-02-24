namespace SchoolManager.Domain.Common.Interfaces;

/// <summary>
/// Contrato base para todos os Domain Events do sistema.
/// Implementado por DomainEventBase — não use diretamente em entidades.
/// </summary>
public interface IDomainEvent
{
    DateTime DateOccurred { get; }
}
