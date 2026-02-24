using SchoolManager.Domain.Common.Base;

namespace SchoolManager.Domain.CoreDomain.Professores.Events;

public sealed record ProfessorCriadoEvent(
    Guid   ProfessorId,
    string PrimeiroNome,
    string Sobrenome) : DomainEventBase;

public sealed record ProfessorExcluidoEvent(Guid ProfessorId) : DomainEventBase;

public sealed record DisciplinaAdicionadaEvent(Guid ProfessorId, Guid DisciplinaId, string NomeDisciplina) : DomainEventBase;

public sealed record DisciplinaRemovidaEvent(Guid ProfessorId, Guid DisciplinaId) : DomainEventBase;
