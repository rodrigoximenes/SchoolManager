using SchoolManager.Domain.Common.Base;
using SchoolManager.Domain.Common.Exceptions;
using SchoolManager.Domain.Common.Validations;
using SchoolManager.Domain.CoreDomain.Mensagens.Events;

namespace SchoolManager.Domain.CoreDomain.Mensagens;

public enum TipoRemetente { Professor = 0, Diretor = 1 }

/// <summary>Destinatário de uma mensagem — entidade filho de Mensagem.</summary>
public sealed class MensagemDestinatario : Entity
{
    public Guid      MensagemId    { get; private set; }
    public Guid      DestinatarioId { get; private set; }
    public bool      Lida          { get; private set; }
    public DateTime? DataLeitura   { get; private set; }

    private MensagemDestinatario(Guid mensagemId, Guid destinatarioId)
    {
        MensagemId     = mensagemId;
        DestinatarioId = destinatarioId;
        Lida           = false;
    }

    private MensagemDestinatario() { }

    internal static MensagemDestinatario Criar(Guid mensagemId, Guid destinatarioId)
        => new(mensagemId, destinatarioId);

    internal void MarcarComoLida()
    {
        Lida        = true;
        DataLeitura = DateTime.UtcNow;
    }
}

/// <summary>Anexo de uma mensagem — entidade filho de Mensagem.</summary>
public sealed class MensagemAnexo : Entity
{
    public Guid   MensagemId    { get; private set; }
    public string Url           { get; private set; } = null!;
    public string NomeArquivo   { get; private set; } = null!;
    public long   TamanhoBytes  { get; private set; }
    public string TipoArquivo   { get; private set; } = null!;

    private static readonly HashSet<string> TiposPermitidos =
        new(StringComparer.OrdinalIgnoreCase) { "jpg", "jpeg", "png", "pdf" };

    private const long MaxTamanhoBytes = 10 * 1024 * 1024; // 10MB

    private MensagemAnexo(Guid mensagemId, string url, string nomeArquivo, long tamanhoBytes, string tipoArquivo)
    {
        Guard.AgainstEmptyGuid(mensagemId, nameof(mensagemId));
        Guard.AgainstNullOrWhiteSpace(url, nameof(url));
        Guard.AgainstNullOrWhiteSpace(nomeArquivo, nameof(nomeArquivo));
        Guard.Against<DomainException>(tamanhoBytes > MaxTamanhoBytes,
            $"Arquivo '{nomeArquivo}' excede o limite de 10MB.");
        Guard.Against<DomainException>(!TiposPermitidos.Contains(tipoArquivo),
            $"Tipo de arquivo '{tipoArquivo}' não permitido. Use: jpg, jpeg, png ou pdf.");

        MensagemId   = mensagemId;
        Url          = url;
        NomeArquivo  = nomeArquivo;
        TamanhoBytes = tamanhoBytes;
        TipoArquivo  = tipoArquivo.ToLowerInvariant();
    }

    private MensagemAnexo() { }

    internal static MensagemAnexo Criar(
        Guid mensagemId, string url, string nomeArquivo, long tamanhoBytes, string tipoArquivo)
        => new(mensagemId, url, nomeArquivo, tamanhoBytes, tipoArquivo);
}

/// <summary>
/// Agregado Mensagem — gerencia envio e leitura de mensagens entre usuários da escola.
/// Remetente pode ser Professor ou Diretor.
/// Destinatários são referenciados por Guid (sem FK — podem ser qualquer tipo de usuário).
/// </summary>
public sealed class Mensagem : AggregateRoot
{
    public Guid           RemetenteId   { get; private set; }
    public TipoRemetente  TipoRemetente { get; private set; }
    public string         Conteudo      { get; private set; } = null!;
    public DateTime       DataEnvio     { get; private set; }

    private readonly List<MensagemDestinatario> _destinatarios = new();
    private readonly List<MensagemAnexo>        _anexos        = new();

    public IReadOnlyCollection<MensagemDestinatario> Destinatarios => _destinatarios.AsReadOnly();
    public IReadOnlyCollection<MensagemAnexo>        Anexos        => _anexos.AsReadOnly();

    private Mensagem(Guid remetenteId, TipoRemetente tipoRemetente, string conteudo)
    {
        Guard.AgainstEmptyGuid(remetenteId, nameof(remetenteId));
        Guard.AgainstNullOrWhiteSpace(conteudo, nameof(conteudo), "Conteúdo da mensagem é obrigatório.");

        RemetenteId   = remetenteId;
        TipoRemetente = tipoRemetente;
        Conteudo      = conteudo.Trim();
        DataEnvio     = DateTime.UtcNow;
    }

    private Mensagem() { }

    public static Mensagem Criar(
        Guid remetenteId,
        TipoRemetente tipoRemetente,
        string conteudo,
        IEnumerable<Guid> destinatariosIds)
    {
        var mensagem = new Mensagem(remetenteId, tipoRemetente, conteudo);

        var ids = destinatariosIds.Distinct().ToList();
        Guard.Against<DomainException>(!ids.Any(), "A mensagem deve ter ao menos um destinatário.");

        foreach (var id in ids)
            mensagem._destinatarios.Add(MensagemDestinatario.Criar(mensagem.Id, id));

        mensagem.AddDomainEvent(new MensagemEnviadaEvent(mensagem.Id, remetenteId, ids.Count));

        return mensagem;
    }

    public void AdicionarAnexo(string url, string nomeArquivo, long tamanhoBytes, string tipoArquivo)
    {
        Guard.Against<DomainException>(IsDeleted, "Não é possível modificar uma mensagem excluída.");

        var anexo = MensagemAnexo.Criar(Id, url, nomeArquivo, tamanhoBytes, tipoArquivo);
        _anexos.Add(anexo);
        SetDataAtualizacao();
    }

    public void MarcarComoLida(Guid destinatarioId)
    {
        Guard.Against<DomainException>(IsDeleted, "Não é possível atualizar uma mensagem excluída.");

        var dest = _destinatarios.FirstOrDefault(d => d.DestinatarioId == destinatarioId);
        Guard.Against<DomainException>(dest is null, "Destinatário não encontrado nesta mensagem.");

        if (dest!.Lida) return; // idempotente

        dest.MarcarComoLida();
        SetDataAtualizacao();
        AddDomainEvent(new MensagemLidaEvent(Id, destinatarioId));
    }

    public void Excluir()
    {
        Guard.Against<DomainException>(IsDeleted, "Mensagem já foi excluída.");
        MarcarComoExcluido();
        AddDomainEvent(new MensagemExcluidaEvent(Id));
    }
}
