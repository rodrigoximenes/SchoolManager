using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Application.Abstractions.Services;
using SchoolManager.Infrastructure.Persistence.Contexts;
using System.Security.Claims;

namespace SchoolManager.Infrastructure.MultiTenancy;

/// <summary>
/// Resolve o tenant (escola) a partir do claim EscolaId no JWT.
/// Busca a ConnectionString no banco Master para rotear o EscolaDbContext.
/// </summary>
public sealed class TenantResolver : ITenantResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly MasterDbContext      _masterContext;

    public TenantResolver(
        IHttpContextAccessor httpContextAccessor,
        MasterDbContext masterContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _masterContext       = masterContext;
    }

    public Guid ObterEscolaId()
    {
        var claim = _httpContextAccessor.HttpContext?.User
            .FindFirstValue("EscolaId");

        if (string.IsNullOrWhiteSpace(claim) || !Guid.TryParse(claim, out var escolaId))
            throw new UnauthorizedAccessException("EscolaId ausente ou inválido no token.");

        return escolaId;
    }

    public string ObterConnectionString(Guid escolaId)
    {
        var escola = _masterContext.Escolas
            .AsNoTracking()
            .FirstOrDefault(e => e.Id == escolaId && e.Ativo);

        if (escola is null)
            throw new InvalidOperationException($"Escola '{escolaId}' não encontrada ou inativa.");

        return escola.ConnectionString;
    }
}
