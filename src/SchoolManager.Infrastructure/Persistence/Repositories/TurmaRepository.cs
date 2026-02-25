using Microsoft.EntityFrameworkCore;
using SchoolManager.Application.Abstractions.Persistence;
using SchoolManager.Application.Common.Pagination;
using SchoolManager.Domain.CoreDomain.Turmas;
using SchoolManager.Infrastructure.Persistence.Contexts;

namespace SchoolManager.Infrastructure.Persistence.Repositories;

public sealed class TurmaRepository : ITurmaRepository
{
    private readonly EscolaDbContext _context;

    public TurmaRepository(EscolaDbContext context) => _context = context;

    public async Task<Turma?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Turmas
            .Include("_professores")
            .Include("_alunos")
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<PagedResult<Turma>> ListarAsync(
        int? periodo, bool? ativo, PagedQuery paginacao, CancellationToken ct = default)
    {
        var query = _context.Turmas.AsQueryable();

        if (periodo.HasValue) query = query.Where(t => t.Periodo == periodo.Value);
        if (ativo.HasValue)   query = query.Where(t => t.Ativo   == ativo.Value);

        var total = await query.CountAsync(ct);
        var itens = await query
            .OrderByDescending(t => t.Periodo)
            .ThenBy(t => t.Turno)
            .Skip(paginacao.Skip)
            .Take(paginacao.Take)
            .ToListAsync(ct);

        return new PagedResult<Turma>(itens, total, paginacao);
    }

    public async Task AdicionarAsync(Turma turma, CancellationToken ct = default)
        => await _context.Turmas.AddAsync(turma, ct);

    public async Task<bool> ExisteAsync(Guid id, CancellationToken ct = default)
        => await _context.Turmas.AnyAsync(t => t.Id == id, ct);

    public Task SalvarAlteracoesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
