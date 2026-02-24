using SchoolManager.Domain.Common.Base;

namespace SchoolManager.Domain.SupportDomain.Matriculas.Events;

public sealed record MatriculaCriadaEvent(Guid MatriculaId, Guid AlunoId) : DomainEventBase;

public sealed record MatriculaCanceladaEvent(
    Guid   MatriculaId,
    Guid   AlunoId,
    string Motivo) : DomainEventBase;

public sealed record MatriculaSuspensaEvent(Guid MatriculaId, Guid AlunoId) : DomainEventBase;
