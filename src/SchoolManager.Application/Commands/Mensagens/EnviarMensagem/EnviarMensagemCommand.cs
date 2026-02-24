using FluentValidation;
using SchoolManager.Application.Abstractions.Persistence;
using SchoolManager.Domain.Common.Exceptions;
using SchoolManager.Domain.CoreDomain.Mensagens;

namespace SchoolManager.Application.Commands.Mensagens.EnviarMensagem;

public sealed class AnexoDto
{
    public string NomeArquivo  { get; }
    public string Url          { get; }
    public long   TamanhoBytes { get; }
    public string TipoArquivo  { get; }

    public AnexoDto(string nomeArquivo, string url, long tamanhoBytes, string tipoArquivo)
    {
        NomeArquivo  = nomeArquivo;
        Url          = url;
        TamanhoBytes = tamanhoBytes;
        TipoArquivo  = tipoArquivo;
    }
}

public sealed class EnviarMensagemCommand
{
    public Guid              RemetenteId    { get; }
    public TipoRemetente     TipoRemetente  { get; }
    public string            Conteudo       { get; }
    public IList<Guid>       DestinatariosIds { get; }
    public IList<AnexoDto>   Anexos         { get; }

    public EnviarMensagemCommand(
        Guid remetenteId,
        TipoRemetente tipoRemetente,
        string conteudo,
        IList<Guid> destinatariosIds,
        IList<AnexoDto>? anexos = null)
    {
        RemetenteId      = remetenteId;
        TipoRemetente    = tipoRemetente;
        Conteudo         = conteudo;
        DestinatariosIds = destinatariosIds;
        Anexos           = anexos ?? new List<AnexoDto>();
    }
}

public sealed class EnviarMensagemResultDto
{
    public Guid     MensagemId          { get; }
    public int      TotalDestinatarios  { get; }
    public int      TotalAnexos         { get; }
    public DateTime DataEnvio           { get; }

    public EnviarMensagemResultDto(Guid id, int destinatarios, int anexos, DateTime dataEnvio)
    {
        MensagemId         = id;
        TotalDestinatarios = destinatarios;
        TotalAnexos        = anexos;
        DataEnvio          = dataEnvio;
    }
}

public sealed class EnviarMensagemCommandHandler
{
    private readonly IMensagemRepository _repo;

    public EnviarMensagemCommandHandler(IMensagemRepository repo) => _repo = repo;

    public async Task<EnviarMensagemResultDto> HandleAsync(
        EnviarMensagemCommand command,
        CancellationToken ct = default)
    {
        if (!command.DestinatariosIds.Any())
            throw new DomainException("A mensagem deve ter ao menos um destinatário.");

        var mensagem = Mensagem.Criar(
            command.RemetenteId,
            command.TipoRemetente,
            command.Conteudo,
            command.DestinatariosIds);

        foreach (var anexo in command.Anexos)
            mensagem.AdicionarAnexo(anexo.Url, anexo.NomeArquivo, anexo.TamanhoBytes, anexo.TipoArquivo);

        await _repo.AdicionarAsync(mensagem, ct);
        await _repo.SalvarAlteracoesAsync(ct);

        return new EnviarMensagemResultDto(
            mensagem.Id,
            mensagem.Destinatarios.Count,
            mensagem.Anexos.Count,
            mensagem.DataEnvio);
    }
}

public sealed class EnviarMensagemRequestValidator : AbstractValidator<EnviarMensagemCommand>
{
    public EnviarMensagemRequestValidator()
    {
        RuleFor(x => x.RemetenteId)
            .NotEmpty().WithMessage("RemetenteId é obrigatório.");

        RuleFor(x => x.Conteudo)
            .NotEmpty().WithMessage("Conteúdo da mensagem é obrigatório.")
            .MaximumLength(10_000).WithMessage("Conteúdo não pode exceder 10.000 caracteres.");

        RuleFor(x => x.DestinatariosIds)
            .NotEmpty().WithMessage("Informe ao menos um destinatário.");
    }
}
