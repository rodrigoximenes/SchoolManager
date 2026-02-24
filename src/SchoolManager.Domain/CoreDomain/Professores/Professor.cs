using SchoolManager.Domain.Common.Base;
using SchoolManager.Domain.Common.Exceptions;
using SchoolManager.Domain.Common.Validations;
using SchoolManager.Domain.CoreDomain.Professores.Events;
using SchoolManager.Domain.CoreDomain.Professores.ValueObjects;

namespace SchoolManager.Domain.CoreDomain.Professores;

/// <summary>
/// Agregado Professor — responsável pela gestão acadêmica do professor.
/// Dados de RH (endereço, salário) ficam em Funcionario no SupportDomain,
/// criado automaticamente via ProfessorCriadoEvent.
/// </summary>
public sealed class Professor : AggregateRoot
{
    public NomePessoa Nome { get; private set; } = null!;

    private readonly List<Disciplina> _disciplinas = new();
    public IReadOnlyCollection<Disciplina> Disciplinas => _disciplinas.AsReadOnly();

    private Professor(NomePessoa nome)
    {
        Guard.AgainstNull(nome, nameof(nome));
        Nome = nome;

        AddDomainEvent(new ProfessorCriadoEvent(Id, nome.PrimeiroNome, nome.Sobrenome));
    }

    private Professor() { }

    public static Professor Criar(string primeiroNome, string sobrenome)
    {
        var nome = NomePessoa.Criar(primeiroNome, sobrenome);
        return new Professor(nome);
    }

    public void AdicionarDisciplina(string nomeDisciplina)
    {
        Guard.Against<DomainException>(IsDeleted, "Não é possível modificar um professor excluído.");
        Guard.AgainstNullOrWhiteSpace(nomeDisciplina, nameof(nomeDisciplina));

        var jaExiste = _disciplinas.Any(d =>
            string.Equals(d.Nome, nomeDisciplina.Trim(), StringComparison.OrdinalIgnoreCase));

        Guard.Against<DomainException>(jaExiste,
            $"Professor já possui a disciplina '{nomeDisciplina}'.");

        var disciplina = Disciplina.Criar(nomeDisciplina);
        _disciplinas.Add(disciplina);
        SetDataAtualizacao();
        AddDomainEvent(new DisciplinaAdicionadaEvent(Id, disciplina.Id, disciplina.Nome));
    }

    public void RemoverDisciplina(Guid disciplinaId)
    {
        Guard.Against<DomainException>(IsDeleted, "Não é possível modificar um professor excluído.");
        Guard.AgainstEmptyGuid(disciplinaId, nameof(disciplinaId));

        var disciplina = _disciplinas.FirstOrDefault(d => d.Id == disciplinaId);
        Guard.Against<DomainException>(disciplina is null,
            "Disciplina não encontrada para este professor.");

        _disciplinas.Remove(disciplina!);
        SetDataAtualizacao();
        AddDomainEvent(new DisciplinaRemovidaEvent(Id, disciplinaId));
    }

    public void Excluir()
    {
        Guard.Against<DomainException>(IsDeleted, "Professor já foi excluído.");
        MarcarComoExcluido();
        AddDomainEvent(new ProfessorExcluidoEvent(Id));
    }
}
