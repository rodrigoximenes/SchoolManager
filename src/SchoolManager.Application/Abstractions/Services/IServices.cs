namespace SchoolManager.Application.Abstractions.Services;

/// <summary>Upload de anexos para Azure Blob Storage.</summary>
public interface IBlobStorageService
{
    /// <returns>URL pública do arquivo enviado.</returns>
    Task<string> UploadAsync(
        Stream   conteudo,
        string   nomeArquivo,
        string   tipoConteudo,
        CancellationToken ct = default);

    Task DeletarAsync(string url, CancellationToken ct = default);
}

/// <summary>Resolve o tenant (escola) a partir do JWT claim EscolaId.</summary>
public interface ITenantResolver
{
    Guid ObterEscolaId();
    string ObterConnectionString(Guid escolaId);
}
