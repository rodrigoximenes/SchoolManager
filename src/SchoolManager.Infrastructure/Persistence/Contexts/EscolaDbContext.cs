using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchoolManager.Domain.Common.Base;
using SchoolManager.Domain.Common.Interfaces;
using SchoolManager.Domain.CoreDomain.Alunos;
using SchoolManager.Domain.CoreDomain.Mensagens;
using SchoolManager.Domain.CoreDomain.Professores;
using SchoolManager.Domain.CoreDomain.Turmas;
using SchoolManager.Domain.SupportDomain.Funcionarios;
using SchoolManager.Domain.SupportDomain.Matriculas;

namespace SchoolManager.Infrastructure.Persistence.Contexts;

/// <summary>
/// DbContext do banco de cada escola (multi-tenancy database-per-tenant).
/// Instanciado com a ConnectionString do tenant resolvida pelo TenantResolver.
/// 
/// Responsabilidades adicionais:
/// 1. Filtro global de soft delete em todas as entidades
/// 2. Despacho de DomainEvents após SaveChangesAsync
/// </summary>
public sealed class EscolaDbContext : DbContext
{
    private readonly IServiceProvider _serviceProvider;

    // CoreDomain
    public DbSet<Turma> Turmas => Set<Turma>();
    public DbSet<Professor> Professores => Set<Professor>();
    public DbSet<Aluno> Alunos => Set<Aluno>();
    public DbSet<Mensagem> Mensagens => Set<Mensagem>();

    // SupportDomain
    public DbSet<Funcionario> Funcionarios => Set<Funcionario>();
    public DbSet<Matricula> Matriculas => Set<Matricula>();

    public EscolaDbContext(
        DbContextOptions<EscolaDbContext> options,
        IServiceProvider serviceProvider) : base(options)
    {
        _serviceProvider = serviceProvider;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica todas as IEntityTypeConfiguration da assembly de Infrastructure
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EscolaDbContext).Assembly);

        // ── Filtro global soft delete ─────────────────────────────────────────
        // Aplica automaticamente em qualquer entidade que herde de Entity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(Entity).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            if (entityType.ClrType.GetProperty("IsDeleted") is null)
            {
                continue;
            }

            modelBuilder.Entity(entityType.ClrType)
                .HasQueryFilter(ExpressionFilterHelper.GetIsDeletedFilter(entityType.ClrType));
        }
    }

    /// <summary>
    /// Override de SaveChangesAsync:
    /// 1. Persiste os dados
    /// 2. Coleta DomainEvents de todas as entidades modificadas
    /// 3. Despacha cada evento para seus handlers registrados no DI
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var result = await base.SaveChangesAsync(ct);
        await DispatchDomainEventsAsync(ct);
        return result;
    }

    private async Task DispatchDomainEventsAsync(CancellationToken ct)
    {
        var entities = ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entities.ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handlers = _serviceProvider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                await (Task)handlerType
                    .GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))!
                    .Invoke(handler, new object[] { domainEvent, ct })!;
            }
        }
    }
}

// Helper para o filtro de soft delete via expression tree
internal static class ExpressionFilterHelper
{
    public static System.Linq.Expressions.LambdaExpression GetIsDeletedFilter(Type entityType)
    {
        var param = System.Linq.Expressions.Expression.Parameter(entityType, "e");
        var property = System.Linq.Expressions.Expression.Property(param, "IsDeleted");
        var notDeleted = System.Linq.Expressions.Expression.Not(property);
        return System.Linq.Expressions.Expression.Lambda(notDeleted, param);
    }
}
