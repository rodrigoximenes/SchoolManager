namespace SchoolManager.Application.Common.Pagination;

/// <summary>
/// Parâmetros de paginação — obrigatório em TODA query que retorna lista.
/// Máximo de 100 itens por página para evitar travamento com grandes volumes.
/// </summary>
public sealed class PagedQuery
{
    private const int MaxTamanhoPagina = 100;

    public int Pagina       { get; }
    public int TamanhoPagina { get; }

    public PagedQuery(int pagina = 1, int tamanhoPagina = 20)
    {
        Pagina        = pagina < 1 ? 1 : pagina;
        TamanhoPagina = tamanhoPagina > MaxTamanhoPagina ? MaxTamanhoPagina
                      : tamanhoPagina < 1               ? 20
                      : tamanhoPagina;
    }

    public int Skip => (Pagina - 1) * TamanhoPagina;
    public int Take => TamanhoPagina;
}

/// <summary>
/// Resultado paginado padronizado para todas as queries de lista.
/// </summary>
public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Itens         { get; }
    public int              TotalItens    { get; }
    public int              Pagina        { get; }
    public int              TamanhoPagina { get; }
    public int              TotalPaginas  { get; }
    public bool             TemProxima    => Pagina < TotalPaginas;
    public bool             TemAnterior   => Pagina > 1;

    public PagedResult(IReadOnlyList<T> itens, int totalItens, PagedQuery query)
    {
        Itens         = itens;
        TotalItens    = totalItens;
        Pagina        = query.Pagina;
        TamanhoPagina = query.TamanhoPagina;
        TotalPaginas  = totalItens == 0 ? 1
                      : (int)Math.Ceiling((double)totalItens / query.TamanhoPagina);
    }

    public static PagedResult<T> Vazio(PagedQuery query)
        => new(Array.Empty<T>(), 0, query);
}
