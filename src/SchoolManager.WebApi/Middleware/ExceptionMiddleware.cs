using System.Net;
using System.Text.Json;
using SchoolManager.Domain.Common.Exceptions;

namespace SchoolManager.WebApi.Middleware;

/// <summary>
/// Captura todas as exceções não tratadas e retorna respostas HTTP padronizadas.
/// Nunca exponha stack traces em produção.
/// 
/// DomainException     → 422 Unprocessable Entity
/// UnauthorizedAccess  → 401
/// InvalidOperation    → 400
/// Exception genérica  → 500 (loga o erro, não expõe detalhes)
/// </summary>
public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next   = next;
        _logger = logger;
        _env    = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("DomainException: {Message}", ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.UnprocessableEntity, "DomainError", ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("UnauthorizedAccess: {Message}", ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.Unauthorized, "Unauthorized", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("InvalidOperation: {Message}", ex.Message);
            await WriteErrorAsync(context, HttpStatusCode.BadRequest, "InvalidOperation", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado em {Path}", context.Request.Path);

            var detail = _env.IsDevelopment() ? ex.ToString() : "Ocorreu um erro interno.";
            await WriteErrorAsync(context, HttpStatusCode.InternalServerError, "InternalServerError", detail);
        }
    }

    private static async Task WriteErrorAsync(
        HttpContext context, HttpStatusCode status, string type, string detail)
    {
        context.Response.StatusCode  = (int)status;
        context.Response.ContentType = "application/problem+json";

        var body = JsonSerializer.Serialize(new
        {
            type,
            title  = status.ToString(),
            status = (int)status,
            detail
        });

        await context.Response.WriteAsync(body);
    }
}
