using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolManager.Application.Abstractions.Persistence;
using SchoolManager.Application.Abstractions.Services;
using SchoolManager.Domain.Common.Interfaces;
using SchoolManager.Domain.CoreDomain.Alunos.Events;
using SchoolManager.Domain.CoreDomain.Professores.Events;
using SchoolManager.Infrastructure.DomainEvents;
using SchoolManager.Infrastructure.MultiTenancy;
using SchoolManager.Infrastructure.Persistence.Contexts;
using SchoolManager.Infrastructure.Persistence.Repositories;

namespace SchoolManager.Infrastructure.DependencyInjection;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Master DbContext ───────────────────────────────────────────────────
        services.AddDbContext<MasterDbContext>(opts =>
            opts.UseNpgsql(configuration.GetConnectionString("Master")));

        // ── Escola DbContext (scoped por request, connection string do tenant) ─
        services.AddScoped<EscolaDbContext>(sp =>
        {
            var tenantResolver = sp.GetRequiredService<ITenantResolver>();
            var escolaId       = tenantResolver.ObterEscolaId();
            var connString     = tenantResolver.ObterConnectionString(escolaId);

            var optionsBuilder = new DbContextOptionsBuilder<EscolaDbContext>();
            optionsBuilder.UseNpgsql(connString);

            return new EscolaDbContext(optionsBuilder.Options, sp);
        });

        // ── Repositórios ──────────────────────────────────────────────────────
        services.AddScoped<ITurmaRepository,      TurmaRepository>();
        services.AddScoped<IProfessorRepository,  ProfessorRepository>();
        services.AddScoped<IAlunoRepository,      AlunoRepository>();
        services.AddScoped<IMensagemRepository,   MensagemRepository>();
        services.AddScoped<IFuncionarioRepository, FuncionarioRepository>();
        services.AddScoped<IMatriculaRepository,  MatriculaRepository>();

        // ── Domain Event Handlers ─────────────────────────────────────────────
        services.AddScoped<IDomainEventHandler<ProfessorCriadoEvent>,  ProfessorCriadoEventHandler>();
        services.AddScoped<IDomainEventHandler<ProfessorExcluidoEvent>, ProfessorExcluidoEventHandler>();
        services.AddScoped<IDomainEventHandler<AlunoCriadoEvent>,       AlunoCriadoEventHandler>();

        // ── MultiTenancy ──────────────────────────────────────────────────────
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantResolver, TenantResolver>();

        return services;
    }
}
