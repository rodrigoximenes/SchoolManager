using Microsoft.EntityFrameworkCore;
using SchoolManager.Application.Abstractions.Persistence;
using SchoolManager.Application.Common.Pagination;
using SchoolManager.Domain.CoreDomain.Alunos;
using SchoolManager.Domain.CoreDomain.Mensagens;
using SchoolManager.Domain.CoreDomain.Professores;
using SchoolManager.Domain.SupportDomain.Funcionarios;
using SchoolManager.Domain.SupportDomain.Matriculas;
using SchoolManager.Infrastructure.Persistence.Contexts;

namespace SchoolManager.Infrastructure.Persistence.Repositories;

// ── ProfessorRepository ───────────────────────────────────────────────────────

public sealed class ProfessorRepository : IProfessorRepository
{
    private readonly EscolaDbContext _ctx;
    public ProfessorRepository(EscolaDbContext ctx) => _ctx = ctx;

    public async Task<Professor?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => await _ctx.Professores.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<PagedResult<Professor>> ListarAsync(string? busca, PagedQuery paginacao, CancellationToken ct = default)
    {
        var q = _ctx.Professores.AsQueryable();
        if (!string.IsNullOrWhiteSpace(busca))
            q = q.Where(p => EF.Property<string>(p, "NomePessoa_Sobrenome").Contains(busca));
        var total = await q.CountAsync(ct);
        var itens = await q.Skip(paginacao.Skip).Take(paginacao.Take).ToListAsync(ct);
        return new PagedResult<Professor>(itens, total, paginacao);
    }

    public async Task AdicionarAsync(Professor professor, CancellationToken ct = default)
        => await _ctx.Professores.AddAsync(professor, ct);

    public Task SalvarAlteracoesAsync(CancellationToken ct = default)
        => _ctx.SaveChangesAsync(ct);
}

// ── AlunoRepository ───────────────────────────────────────────────────────────

public sealed class AlunoRepository : IAlunoRepository
{
    private readonly EscolaDbContext _ctx;
    public AlunoRepository(EscolaDbContext ctx) => _ctx = ctx;

    public async Task<Aluno?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => await _ctx.Alunos.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<PagedResult<Aluno>> ListarPorTurmaAsync(Guid turmaId, PagedQuery paginacao, CancellationToken ct = default)
    {
        // Turma carrega a lista de AlunoIds; aqui buscamos os Alunos correspondentes
        var turma = await _ctx.Turmas.FirstOrDefaultAsync(t => t.Id == turmaId, ct);
        if (turma is null) return PagedResult<Aluno>.Vazio(paginacao);

        var ids   = turma.Alunos.Select(a => a.AlunoId).ToList();
        var total = ids.Count;
        var itens = await _ctx.Alunos
            .Where(a => ids.Contains(a.Id))
            .Skip(paginacao.Skip).Take(paginacao.Take)
            .ToListAsync(ct);

        return new PagedResult<Aluno>(itens, total, paginacao);
    }

    public async Task<PagedResult<Aluno>> ListarAsync(string? busca, PagedQuery paginacao, CancellationToken ct = default)
    {
        var q = _ctx.Alunos.AsQueryable();
        if (!string.IsNullOrWhiteSpace(busca))
            q = q.Where(a => a.Nome.Contains(busca));
        var total = await q.CountAsync(ct);
        var itens = await q.OrderBy(a => a.Nome).Skip(paginacao.Skip).Take(paginacao.Take).ToListAsync(ct);
        return new PagedResult<Aluno>(itens, total, paginacao);
    }

    public async Task AdicionarAsync(Aluno aluno, CancellationToken ct = default)
        => await _ctx.Alunos.AddAsync(aluno, ct);

    public Task SalvarAlteracoesAsync(CancellationToken ct = default)
        => _ctx.SaveChangesAsync(ct);
}

// ── MensagemRepository ────────────────────────────────────────────────────────

public sealed class MensagemRepository : IMensagemRepository
{
    private readonly EscolaDbContext _ctx;
    public MensagemRepository(EscolaDbContext ctx) => _ctx = ctx;

    public async Task<Mensagem?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => await _ctx.Mensagens.FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<PagedResult<Mensagem>> ListarRecebidasAsync(Guid destinatarioId, PagedQuery paginacao, CancellationToken ct = default)
    {
        var q = _ctx.Mensagens.Where(m => m.Destinatarios.Any(d => d.DestinatarioId == destinatarioId));
        var total = await q.CountAsync(ct);
        var itens = await q.OrderByDescending(m => m.DataEnvio).Skip(paginacao.Skip).Take(paginacao.Take).ToListAsync(ct);
        return new PagedResult<Mensagem>(itens, total, paginacao);
    }

    public async Task<PagedResult<Mensagem>> ListarEnviadasAsync(Guid remetenteId, PagedQuery paginacao, CancellationToken ct = default)
    {
        var q = _ctx.Mensagens.Where(m => m.RemetenteId == remetenteId);
        var total = await q.CountAsync(ct);
        var itens = await q.OrderByDescending(m => m.DataEnvio).Skip(paginacao.Skip).Take(paginacao.Take).ToListAsync(ct);
        return new PagedResult<Mensagem>(itens, total, paginacao);
    }

    public async Task AdicionarAsync(Mensagem mensagem, CancellationToken ct = default)
        => await _ctx.Mensagens.AddAsync(mensagem, ct);

    public Task SalvarAlteracoesAsync(CancellationToken ct = default)
        => _ctx.SaveChangesAsync(ct);
}

// ── FuncionarioRepository ─────────────────────────────────────────────────────

public sealed class FuncionarioRepository : IFuncionarioRepository
{
    private readonly EscolaDbContext _ctx;
    public FuncionarioRepository(EscolaDbContext ctx) => _ctx = ctx;

    public async Task<Funcionario?> ObterPorProfessorIdAsync(Guid professorId, CancellationToken ct = default)
        => await _ctx.Funcionarios.FirstOrDefaultAsync(f => f.ProfessorId == professorId, ct);

    public async Task AdicionarAsync(Funcionario funcionario, CancellationToken ct = default)
        => await _ctx.Funcionarios.AddAsync(funcionario, ct);

    public Task SalvarAlteracoesAsync(CancellationToken ct = default)
        => _ctx.SaveChangesAsync(ct);
}

// ── MatriculaRepository ───────────────────────────────────────────────────────

public sealed class MatriculaRepository : IMatriculaRepository
{
    private readonly EscolaDbContext _ctx;
    public MatriculaRepository(EscolaDbContext ctx) => _ctx = ctx;

    public async Task<Matricula?> ObterPorAlunoIdAsync(Guid alunoId, CancellationToken ct = default)
        => await _ctx.Matriculas.FirstOrDefaultAsync(m => m.AlunoId == alunoId, ct);

    public async Task<PagedResult<Matricula>> ListarAsync(string? status, PagedQuery paginacao, CancellationToken ct = default)
    {
        var q = _ctx.Matriculas.AsQueryable();
        var total = await q.CountAsync(ct);
        var itens = await q.OrderByDescending(m => m.DataMatricula).Skip(paginacao.Skip).Take(paginacao.Take).ToListAsync(ct);
        return new PagedResult<Matricula>(itens, total, paginacao);
    }

    public async Task AdicionarAsync(Matricula matricula, CancellationToken ct = default)
        => await _ctx.Matriculas.AddAsync(matricula, ct);

    public Task SalvarAlteracoesAsync(CancellationToken ct = default)
        => _ctx.SaveChangesAsync(ct);
}
