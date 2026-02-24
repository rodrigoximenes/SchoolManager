using SchoolManager.Domain.Common.Base;
using SchoolManager.Domain.Common.Validations;

namespace SchoolManager.Domain.CoreDomain.Alunos;

/// <summary>
/// Entidade filho de Aluno — acessada apenas através do agregado.
/// Representa o responsável legal pelo aluno.
/// </summary>
public sealed class AlunoResponsavel : Entity
{
    public Guid    AlunoId    { get; private set; }
    public string  Nome       { get; private set; } = null!;
    public string? Parentesco { get; private set; }
    public string? Telefone   { get; private set; }

    private AlunoResponsavel(Guid alunoId, string nome, string? parentesco, string? telefone)
    {
        Guard.AgainstEmptyGuid(alunoId, nameof(alunoId));
        Guard.AgainstNullOrWhiteSpace(nome, nameof(nome), "Nome do responsável é obrigatório.");
        Guard.Against<ArgumentException>(nome.Length > 300, "Nome do responsável não pode ter mais de 300 caracteres.");

        AlunoId    = alunoId;
        Nome       = nome.Trim();
        Parentesco = parentesco?.Trim();
        Telefone   = telefone?.Trim();
    }

    private AlunoResponsavel() { }

    internal static AlunoResponsavel Criar(Guid alunoId, string nome, string? parentesco, string? telefone)
        => new(alunoId, nome, parentesco, telefone);
}
