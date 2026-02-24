using SchoolManager.Domain.Common.Base;
using SchoolManager.Domain.Common.Exceptions;
using SchoolManager.Domain.Common.Validations;
using SchoolManager.Domain.CoreDomain.Alunos.Events;
using SchoolManager.Domain.CoreDomain.Alunos.ValueObjects;

namespace SchoolManager.Domain.CoreDomain.Alunos;

/// <summary>
/// Agregado Aluno — gerencia dados acadêmicos do aluno.
/// Dados de matrícula e financeiros ficam no SupportDomain,
/// criados automaticamente via AlunoCriadoEvent.
/// </summary>
public sealed class Aluno : AggregateRoot
{
    public string Nome { get; private set; } = null!;
    public DateOnly? DataNascimento { get; private set; }

    private readonly List<AlunoResponsavel> _responsaveis = new();
    private readonly List<Nota> _notas = new();
    private readonly List<Presenca> _presencas = new();

    public IReadOnlyCollection<AlunoResponsavel> Responsaveis => _responsaveis.AsReadOnly();
    public IReadOnlyCollection<Nota> Notas => _notas.AsReadOnly();
    public IReadOnlyCollection<Presenca> Presencas => _presencas.AsReadOnly();

    private Aluno(string nome, DateOnly? dataNascimento)
    {
        Guard.AgainstNullOrWhiteSpace(nome, nameof(nome), "Nome do aluno é obrigatório.");
        Guard.Against<ArgumentException>(nome.Length > 300, "Nome não pode ter mais de 300 caracteres.");

        Nome = nome.Trim();
        DataNascimento = dataNascimento;

        AddDomainEvent(new AlunoCriadoEvent(Id, Nome));
    }

    private Aluno() { }

    public static Aluno Criar(string nome, DateOnly? dataNascimento = null)
        => new(nome, dataNascimento);

    // ── Comportamentos ────────────────────────────────────────────────────────

    public void AdicionarResponsavel(string nome, string? parentesco, string? telefone)
    {
        Guard.Against<DomainException>(IsDeleted, "Não é possível modificar um aluno excluído.");
        Guard.AgainstNullOrWhiteSpace(nome, nameof(nome), "Nome do responsável é obrigatório.");

        var responsavel = AlunoResponsavel.Criar(Id, nome, parentesco, telefone);
        _responsaveis.Add(responsavel);
        SetDataAtualizacao();
    }

    public void LancarNota(Guid disciplinaId, decimal valor, DateOnly data)
    {
        Guard.Against<DomainException>(IsDeleted, "Não é possível lançar nota para aluno excluído.");

        var nota = Nota.Criar(disciplinaId, valor, data);
        _notas.Add(nota);
        SetDataAtualizacao();
        AddDomainEvent(new NotaLancadaEvent(Id, disciplinaId, valor, data));
    }

    public void RegistrarPresenca(Guid disciplinaId, DateOnly data, bool presente)
    {
        Guard.Against<DomainException>(IsDeleted, "Não é possível registrar presença para aluno excluído.");

        var jaRegistrada = _presencas.Any(p =>
            p.DisciplinaId == disciplinaId && p.Data == data);

        Guard.Against<DomainException>(jaRegistrada,
            $"Presença já registrada para esta disciplina na data {data:dd/MM/yyyy}.");

        var presenca = Presenca.Criar(disciplinaId, data, presente);
        _presencas.Add(presenca);
        SetDataAtualizacao();
        AddDomainEvent(new PresencaRegistradaEvent(Id, disciplinaId, data, presente));
    }

    public void Excluir()
    {
        Guard.Against<DomainException>(IsDeleted, "Aluno já foi excluído.");
        MarcarComoExcluido();
        AddDomainEvent(new AlunoExcluidoEvent(Id));
    }
}
