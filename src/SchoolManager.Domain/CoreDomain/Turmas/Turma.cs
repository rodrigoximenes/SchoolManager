using SchoolManager.Domain.Common.Base;
using SchoolManager.Domain.Common.Exceptions;
using SchoolManager.Domain.Common.Validations;
using SchoolManager.Domain.CoreDomain.Turmas.Enums;
using SchoolManager.Domain.CoreDomain.Turmas.Events;

namespace SchoolManager.Domain.CoreDomain.Turmas;

/// <summary>
/// Entidade de vínculo Professor↔Turma.
/// EF Core requer uma entidade real (não Guid primitivo) para OwnsMany.
/// </summary>
public sealed class TurmaProfessor
{
    public Guid     TurmaId     { get; private set; }
    public Guid     ProfessorId { get; private set; }
    public DateTime DataVinculo { get; private set; }

    private TurmaProfessor() { }

    internal TurmaProfessor(Guid turmaId, Guid professorId)
    {
        TurmaId     = turmaId;
        ProfessorId = professorId;
        DataVinculo = DateTime.UtcNow;
    }
}

/// <summary>
/// Entidade de vínculo Aluno↔Turma.
/// </summary>
public sealed class TurmaAluno
{
    public Guid     TurmaId     { get; private set; }
    public Guid     AlunoId     { get; private set; }
    public DateTime DataVinculo { get; private set; }

    private TurmaAluno() { }

    internal TurmaAluno(Guid turmaId, Guid alunoId)
    {
        TurmaId     = turmaId;
        AlunoId     = alunoId;
        DataVinculo = DateTime.UtcNow;
    }
}

/// <summary>
/// Agregado Turma — representa uma turma ativa em um período letivo.
///
/// Regras de negócio:
/// - Turno e Periodo são imutáveis após criação
/// - Professores e Alunos só podem ser adicionados/removidos em turmas ativas
/// - Turma só pode ser excluída se estiver inativa
/// - Não permite duplicidade de professor ou aluno na mesma turma
/// </summary>
public sealed class Turma : AggregateRoot
{
    public Turno Turno   { get; private set; }
    public int   Periodo { get; private set; }
    public bool  Ativo   { get; private set; }

    private readonly List<TurmaProfessor> _professores = new();
    private readonly List<TurmaAluno>     _alunos      = new();

    public IReadOnlyCollection<TurmaProfessor> Professores => _professores.AsReadOnly();
    public IReadOnlyCollection<TurmaAluno>     Alunos      => _alunos.AsReadOnly();

    // ── Construtor privado — use o Factory Criar() ────────────────────────────

    private Turma(Turno turno, int periodo)
    {
        Guard.AgainstOutOfRange(periodo, 2000, 2100, nameof(periodo),
            "Período deve ser um ano letivo válido (entre 2000 e 2100).");

        Turno   = turno;
        Periodo = periodo;
        Ativo   = true;

        AddDomainEvent(new TurmaCriadaEvent(Id, Periodo));
    }

    private Turma() { }

    // ── Factory ───────────────────────────────────────────────────────────────

    public static Turma Criar(Turno turno, int periodo) => new(turno, periodo);

    // ── Comportamentos ────────────────────────────────────────────────────────

    public void AdicionarProfessor(Guid professorId)
    {
        Guard.AgainstEmptyGuid(professorId, nameof(professorId));
        Guard.Against<DomainException>(!Ativo,    "Não é possível adicionar professor em turma inativa.");
        Guard.Against<DomainException>(IsDeleted, "Não é possível modificar uma turma excluída.");

        if (_professores.Any(p => p.ProfessorId == professorId)) return; // idempotente

        _professores.Add(new TurmaProfessor(Id, professorId));
        SetDataAtualizacao();
        AddDomainEvent(new ProfessorAdicionadoTurmaEvent(Id, professorId));
    }

    public void RemoverProfessor(Guid professorId)
    {
        Guard.AgainstEmptyGuid(professorId, nameof(professorId));
        Guard.Against<DomainException>(!Ativo,    "Não é possível remover professor de turma inativa.");
        Guard.Against<DomainException>(IsDeleted, "Não é possível modificar uma turma excluída.");

        var vinculo = _professores.FirstOrDefault(p => p.ProfessorId == professorId);
        Guard.Against<DomainException>(vinculo is null, "Professor não encontrado nesta turma.");

        _professores.Remove(vinculo!);
        SetDataAtualizacao();
        AddDomainEvent(new ProfessorRemovidoTurmaEvent(Id, professorId));
    }

    public void AdicionarAluno(Guid alunoId)
    {
        Guard.AgainstEmptyGuid(alunoId, nameof(alunoId));
        Guard.Against<DomainException>(!Ativo,    "Não é possível adicionar aluno em turma inativa.");
        Guard.Against<DomainException>(IsDeleted, "Não é possível modificar uma turma excluída.");

        if (_alunos.Any(a => a.AlunoId == alunoId)) return; // idempotente

        _alunos.Add(new TurmaAluno(Id, alunoId));
        SetDataAtualizacao();
        AddDomainEvent(new AlunoAdicionadoTurmaEvent(Id, alunoId));
    }

    public void RemoverAluno(Guid alunoId)
    {
        Guard.AgainstEmptyGuid(alunoId, nameof(alunoId));
        Guard.Against<DomainException>(!Ativo,    "Não é possível remover aluno de turma inativa.");
        Guard.Against<DomainException>(IsDeleted, "Não é possível modificar uma turma excluída.");

        var vinculo = _alunos.FirstOrDefault(a => a.AlunoId == alunoId);
        Guard.Against<DomainException>(vinculo is null, "Aluno não encontrado nesta turma.");

        _alunos.Remove(vinculo!);
        SetDataAtualizacao();
        AddDomainEvent(new AlunoRemovidoTurmaEvent(Id, alunoId));
    }

    public void Desativar()
    {
        Guard.Against<DomainException>(!Ativo,    "Turma já está inativa.");
        Guard.Against<DomainException>(IsDeleted, "Não é possível desativar uma turma excluída.");

        Ativo = false;
        SetDataAtualizacao();
        AddDomainEvent(new TurmaDesativadaEvent(Id));
    }

    public void Excluir()
    {
        Guard.Against<DomainException>(Ativo,     "Desative a turma antes de excluí-la.");
        Guard.Against<DomainException>(IsDeleted, "Turma já foi excluída.");

        MarcarComoExcluido();
        AddDomainEvent(new TurmaExcluidaEvent(Id));
    }
}
