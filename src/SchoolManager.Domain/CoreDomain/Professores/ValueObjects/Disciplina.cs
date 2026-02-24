using SchoolManager.Domain.Common.Base;
using SchoolManager.Domain.Common.Validations;

namespace SchoolManager.Domain.CoreDomain.Professores.ValueObjects;

/// <summary>
/// Value Object que representa uma disciplina lecionada por um professor.
/// Mapeado como OwnsMany no EF Core — tabela separada Disciplinas.
/// Possui Id próprio para permitir remoção individual via EF Core.
/// </summary>
public sealed class Disciplina : ValueObject
{
    public Guid   Id   { get; }
    public string Nome { get; }

    private Disciplina(string nome)
    {
        Guard.AgainstNullOrWhiteSpace(nome, nameof(nome), "Nome da disciplina é obrigatório.");
        Guard.Against<ArgumentException>(nome.Length > 150, "Nome da disciplina não pode ter mais de 150 caracteres.");

        Id   = Guid.NewGuid();
        Nome = nome.Trim();
    }

    // Construtor para EF Core
    private Disciplina() { Nome = string.Empty; }

    public static Disciplina Criar(string nome) => new(nome);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Nome.ToUpperInvariant();
    }
}
