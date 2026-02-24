namespace SchoolManager.Domain.Common.Base;

/// <summary>
/// Objeto de valor — definido pelos seus atributos, não por identidade.
/// Imutável por design. Mapeado como Owned Type no EF Core (OwnsOne / OwnsMany).
/// 
/// Exemplo de uso: Endereco, NomePessoa, Disciplina, Nota, Presenca
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Retorna todos os atributos que definem a igualdade estrutural.
    /// Implemente em cada subclasse incluindo todos os campos relevantes.
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType()) return false;
        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
        => GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);

    public static bool operator ==(ValueObject? a, ValueObject? b)
        => a?.Equals(b) ?? b is null;

    public static bool operator !=(ValueObject? a, ValueObject? b)
        => !(a == b);
}
