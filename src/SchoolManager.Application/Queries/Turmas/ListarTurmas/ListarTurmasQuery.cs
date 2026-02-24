using SchoolManager.Application.Abstractions.Persistence;
using SchoolManager.Application.Common.Pagination;
using SchoolManager.Domain.CoreDomain.Turmas;

namespace SchoolManager.Application.Queries.Turmas.ListarTurmas;

public sealed class ListarTurmasQuery
{
    public int?      Periodo      { get; }
    public bool?     Ativo        { get; }
    public PagedQuery Paginacao   { get; }

    public ListarTurmasQuery(int? periodo, bool? ativo, int pagina = 1, int tamanhoPagina = 20)
    {
        Periodo   = periodo;
        Ativo     = ativo;
        Paginacao = new PagedQuery(pagina, tamanhoPagina);
    }
}

public sealed class TurmaResumoDto
{
    public Guid   TurmaId           { get; }
    public string Turno             { get; }
    public int    Periodo           { get; }
    public bool   Ativo             { get; }
    public int    TotalProfessores  { get; }
    public int    TotalAlunos       { get; }

    private TurmaResumoDto(Guid id, string turno, int periodo, bool ativo, int professores, int alunos)
    {
        TurmaId          = id;
        Turno            = turno;
        Periodo          = periodo;
        Ativo            = ativo;
        TotalProfessores = professores;
        TotalAlunos      = alunos;
    }

    public static TurmaResumoDto FromDomain(Turma t)
        => new(t.Id, t.Turno.ToString(), t.Periodo, t.Ativo,
               t.Professores.Count, t.Alunos.Count);
}

public sealed class ListarTurmasQueryHandler
{
    private readonly ITurmaRepository _repo;

    public ListarTurmasQueryHandler(ITurmaRepository repo) => _repo = repo;

    public async Task<PagedResult<TurmaResumoDto>> HandleAsync(
        ListarTurmasQuery query,
        CancellationToken ct = default)
    {
        var resultado = await _repo.ListarAsync(query.Periodo, query.Ativo, query.Paginacao, ct);

        var dtos = resultado.Itens
            .Select(TurmaResumoDto.FromDomain)
            .ToList()
            .AsReadOnly();

        return new PagedResult<TurmaResumoDto>(dtos, resultado.TotalItens, query.Paginacao);
    }
}
