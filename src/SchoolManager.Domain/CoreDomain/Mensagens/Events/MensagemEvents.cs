using SchoolManager.Domain.Common.Base;

namespace SchoolManager.Domain.CoreDomain.Mensagens.Events;

public sealed record MensagemEnviadaEvent(
    Guid   MensagemId,
    Guid   RemetenteId,
    int    TotalDestinatarios) : DomainEventBase;

public sealed record MensagemLidaEvent(
    Guid MensagemId,
    Guid DestinatarioId) : DomainEventBase;

public sealed record MensagemExcluidaEvent(Guid MensagemId) : DomainEventBase;
