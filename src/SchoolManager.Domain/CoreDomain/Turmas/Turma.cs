using SchoolManager.Domain.Common.Base;
using SchoolManager.Domain.Common.Exceptions;
using SchoolManager.Domain.Common.Validations;
using SchoolManager.Domain.CoreDomain.Turmas.Enums;
using SchoolManager.Domain.CoreDomain.Turmas.Events;

namespace SchoolManager.Domain.CoreDomain.Turmas;

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

    // Coleções internas — expostas como somente leitura
    private readonly List<Guid> _professores = new();
    private readonly List<Guid> _alunos      = new();

    public IReadOnlyCollection<Guid> Professores => _professores.AsReadOnly();
    public IReadOnlyCollection<Guid> Alunos      => _alunos.AsReadOnly();

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

    // Construtor protegido para reconstituição via EF Core
    private Turma() { }

    // ── Factory ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Cria uma nova turma. Único ponto de entrada para instanciar Turma.
    /// </summary>
    public static Turma Criar(Turno turno, int periodo)
        => new(turno, periodo);

    // ── Comportamentos ────────────────────────────────────────────────────────

    /// <summary>
    /// Adiciona um professor à turma.
    /// Idempotente — silencioso se o professor já estiver na turma.
    /// </summary>
    public void AdicionarProfessor(Guid professorId)
    {
        Guard.AgainstEmptyGuid(professorId, nameof(professorId));
        Guard.Against<DomainException>(!Ativo,     "Não é possível adicionar professor em turma inativa.");
        Guard.Against<DomainException>(IsDeleted,  "Não é possível modificar uma turma excluída.");

        if (_professores.Contains(professorId)) return;

        _professores.Add(professorId);
        SetDataAtualizacao();
        AddDomainEvent(new ProfessorAdicionadoTurmaEvent(Id, professorId));
    }

    /// <summary>
    /// Remove um professor da turma.
    /// </summary>
    public void RemoverProfessor(Guid professorId)
    {
        Guard.AgainstEmptyGuid(professorId, nameof(professorId));
        Guard.Against<DomainException>(!Ativo,    "Não é possível remover professor de turma inativa.");
        Guard.Against<DomainException>(IsDeleted, "Não é possível modificar uma turma excluída.");
        Guard.Against<DomainException>(
            !_professores.Contains(professorId),
            "Professor não encontrado nesta turma.");

        _professores.Remove(professorId);
        SetDataAtualizacao();
        AddDomainEvent(new ProfessorRemovidoTurmaEvent(Id, professorId));
    }

    /// <summary>
    /// Adiciona um aluno à turma.
    /// Idempotente — silencioso se o aluno já estiver na turma.
    /// </summary>
    public void AdicionarAluno(Guid alunoId)
    {
        Guard.AgainstEmptyGuid(alunoId, nameof(alunoId));
        Guard.Against<DomainException>(!Ativo,    "Não é possível adicionar aluno em turma inativa.");
        Guard.Against<DomainException>(IsDeleted, "Não é possível modificar uma turma excluída.");

        if (_alunos.Contains(alunoId)) return;

        _alunos.Add(alunoId);
        SetDataAtualizacao();
        AddDomainEvent(new AlunoAdicionadoTurmaEvent(Id, alunoId));
    }

    /// <summary>
    /// Remove um aluno da turma.
    /// </summary>
    public void RemoverAluno(Guid alunoId)
    {
        Guard.AgainstEmptyGuid(alunoId, nameof(alunoId));
        Guard.Against<DomainException>(!Ativo,    "Não é possível remover aluno de turma inativa.");
        Guard.Against<DomainException>(IsDeleted, "Não é possível modificar uma turma excluída.");
        Guard.Against<DomainException>(
            !_alunos.Contains(alunoId),
            "Aluno não encontrado nesta turma.");

        _alunos.Remove(alunoId);
        SetDataAtualizacao();
        AddDomainEvent(new AlunoRemovidoTurmaEvent(Id, alunoId));
    }

    /// <summary>
    /// Desativa a turma — pré-requisito para excluir.
    /// </summary>
    public void Desativar()
    {
        Guard.Against<DomainException>(!Ativo,    "Turma já está inativa.");
        Guard.Against<DomainException>(IsDeleted, "Não é possível desativar uma turma excluída.");

        Ativo = false;
        SetDataAtualizacao();
        AddDomainEvent(new TurmaDesativadaEvent(Id));
    }

    /// <summary>
    /// Soft delete — só permitido em turmas inativas.
    /// </summary>
    public void Excluir()
    {
        Guard.Against<DomainException>(Ativo,     "Desative a turma antes de excluí-la.");
        Guard.Against<DomainException>(IsDeleted, "Turma já foi excluída.");

        MarcarComoExcluido();
        AddDomainEvent(new TurmaExcluidaEvent(Id));
    }
}
