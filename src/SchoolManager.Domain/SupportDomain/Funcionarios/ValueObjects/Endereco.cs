using SchoolManager.Domain.Common.Base;
using SchoolManager.Domain.Common.Validations;

namespace SchoolManager.Domain.SupportDomain.Funcionarios.ValueObjects;

/// <summary>
/// Value Object Endereço — compartilhado entre Funcionario e Matricula.
/// Mapeado como OwnedOne no EF Core com prefixo de coluna configurado na Configuration.
/// </summary>
public sealed class Endereco : ValueObject
{
    public string  Logradouro { get; }
    public string  Numero     { get; }
    public string  Bairro     { get; }
    public string  Cidade     { get; }
    public string  Estado     { get; }
    public string  CEP        { get; }

    private Endereco(string logradouro, string numero, string bairro,
                     string cidade, string estado, string cep)
    {
        Guard.AgainstNullOrWhiteSpace(logradouro, nameof(logradouro));
        Guard.AgainstNullOrWhiteSpace(numero,     nameof(numero));
        Guard.AgainstNullOrWhiteSpace(bairro,     nameof(bairro));
        Guard.AgainstNullOrWhiteSpace(cidade,     nameof(cidade));
        Guard.AgainstNullOrWhiteSpace(estado,     nameof(estado));
        Guard.AgainstNullOrWhiteSpace(cep,        nameof(cep));
        Guard.Against<ArgumentException>(estado.Length != 2, "Estado deve ter exatamente 2 caracteres (UF).");

        Logradouro = logradouro.Trim();
        Numero     = numero.Trim();
        Bairro     = bairro.Trim();
        Cidade     = cidade.Trim();
        Estado     = estado.Trim().ToUpperInvariant();
        CEP        = cep.Trim();
    }

    private Endereco()
    {
        Logradouro = Numero = Bairro = Cidade = Estado = CEP = string.Empty;
    }

    public static Endereco Criar(string logradouro, string numero, string bairro,
                                  string cidade, string estado, string cep)
        => new(logradouro, numero, bairro, cidade, estado, cep);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Logradouro.ToUpperInvariant();
        yield return Numero;
        yield return CEP;
    }
}
