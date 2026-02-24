using SchoolManager.Domain.Common.Base;

namespace SchoolManager.Domain.CoreDomain.Turmas.Events;

public sealed record TurmaCriadaEvent(Guid TurmaId, int Periodo) : DomainEventBase;

public sealed record TurmaDesativadaEvent(Guid TurmaId) : DomainEventBase;

public sealed record TurmaExcluidaEvent(Guid TurmaId) : DomainEventBase;

public sealed record ProfessorAdicionadoTurmaEvent(Guid TurmaId, Guid ProfessorId) : DomainEventBase;

public sealed record ProfessorRemovidoTurmaEvent(Guid TurmaId, Guid ProfessorId) : DomainEventBase;

public sealed record AlunoAdicionadoTurmaEvent(Guid TurmaId, Guid AlunoId) : DomainEventBase;

public sealed record AlunoRemovidoTurmaEvent(Guid TurmaId, Guid AlunoId) : DomainEventBase;
