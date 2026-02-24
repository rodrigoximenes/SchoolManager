using SchoolManager.Domain.Common.Base;
using SchoolManager.Domain.Common.Validations;

namespace SchoolManager.Domain.CoreDomain.Professores.ValueObjects;

/// <summary>
/// Value Object que representa o nome de uma pessoa.
/// Mapeado como OwnedOne no EF Core — colunas: NomePessoa_PrimeiroNome, NomePessoa_Sobrenome.
/// </summary>
public sealed class NomePessoa : ValueObject
{
    public string PrimeiroNome { get; }
    public string Sobrenome    { get; }

    public string NomeCompleto => $"{PrimeiroNome} {Sobrenome}";

    private NomePessoa(string primeiroNome, string sobrenome)
    {
        Guard.AgainstNullOrWhiteSpace(primeiroNome, nameof(primeiroNome), "Primeiro nome é obrigatório.");
        Guard.AgainstNullOrWhiteSpace(sobrenome,    nameof(sobrenome),    "Sobrenome é obrigatório.");
        Guard.Against<ArgumentException>(primeiroNome.Length > 100, "Primeiro nome não pode ter mais de 100 caracteres.");
        Guard.Against<ArgumentException>(sobrenome.Length    > 200, "Sobrenome não pode ter mais de 200 caracteres.");

        PrimeiroNome = primeiroNome.Trim();
        Sobrenome    = sobrenome.Trim();
    }

    // Construtor sem parâmetros para EF Core
    private NomePessoa() { PrimeiroNome = string.Empty; Sobrenome = string.Empty; }

    public static NomePessoa Criar(string primeiroNome, string sobrenome)
        => new(primeiroNome, sobrenome);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return PrimeiroNome.ToUpperInvariant();
        yield return Sobrenome.ToUpperInvariant();
    }
}
