namespace SchoolManager.Domain.Common.Base;

/// <summary>
/// Raiz de agregado — ponto de entrada para todas as operações do agregado.
/// Apenas AggregateRoots têm repositórios. Entidades internas são acessadas
/// somente através da raiz.
/// </summary>
public abstract class AggregateRoot : Entity
{
    protected AggregateRoot() : base() { }
    protected AggregateRoot(Guid id) : base(id) { }
}
