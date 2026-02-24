using SchoolManager.Domain.Common.Base;

namespace SchoolManager.Domain.CoreDomain.Alunos.Events;

public sealed record AlunoCriadoEvent(Guid AlunoId, string Nome) : DomainEventBase;

public sealed record AlunoExcluidoEvent(Guid AlunoId) : DomainEventBase;

public sealed record NotaLancadaEvent(
    Guid    AlunoId,
    Guid    DisciplinaId,
    decimal Valor,
    DateOnly Data) : DomainEventBase;

public sealed record PresencaRegistradaEvent(
    Guid     AlunoId,
    Guid     DisciplinaId,
    DateOnly Data,
    bool     Presente) : DomainEventBase;
