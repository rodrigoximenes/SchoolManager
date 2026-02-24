using SchoolManager.Domain.Common.Interfaces;

namespace SchoolManager.Domain.Common.Base;

/// <summary>
/// Base para todas as entidades do domínio.
/// 
/// Regras:
/// - Id gerado no construtor — nunca pelo banco
/// - private sets em todas as propriedades
/// - Igualdade baseada apenas no Id
/// - Soft delete via IsDeleted + DataExclusao
/// - DomainEvents coletados e despachados pelo EscolaDbContext após SaveChanges
/// </summary>
public abstract class Entity
{
    public Guid      Id              { get; protected set; }
    public DateTime  DataCriacao     { get; protected set; }
    public DateTime? DataAtualizacao { get; protected set; }
    public bool      IsDeleted       { get; protected set; }
    public DateTime? DataExclusao    { get; protected set; }

    protected Entity()
    {
        Id          = Guid.NewGuid();
        DataCriacao = DateTime.UtcNow;
    }

    // Construtor para reconstituição via ORM (EF Core)
    protected Entity(Guid id)
    {
        Id = id;
    }

    protected void SetDataAtualizacao()
        => DataAtualizacao = DateTime.UtcNow;

    /// <summary>
    /// Soft delete — nunca deletar fisicamente entidades de domínio.
    /// </summary>
    protected void MarcarComoExcluido()
    {
        IsDeleted    = true;
        DataExclusao = DateTime.UtcNow;
        SetDataAtualizacao();
    }

    // ── Igualdade ─────────────────────────────────────────────────────────────

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id.Equals(other.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity? left, Entity? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(Entity? left, Entity? right)
        => !(left == right);

    // ── Domain Events ─────────────────────────────────────────────────────────

    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Eventos gerados durante a operação. 
    /// O EscolaDbContext os despacha após SaveChangesAsync.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents
        => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    public void RemoveDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Remove(domainEvent);

    public void ClearDomainEvents()
        => _domainEvents.Clear();
}
