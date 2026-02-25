using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManager.Application.Commands.Turmas.CriarTurma;
using SchoolManager.Application.Queries.Turmas.ListarTurmas;
using SchoolManager.Domain.CoreDomain.Turmas.Enums;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SchoolManager.WebApi.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v1/turmas")]
[Authorize]
public sealed class TurmaController : ControllerBase
{
    private readonly CriarTurmaCommandHandler    _criarHandler;
    private readonly ListarTurmasQueryHandler    _listarHandler;
    private readonly IValidator<CriarTurmaCommand> _validator;

    public TurmaController(
        CriarTurmaCommandHandler criarHandler,
        ListarTurmasQueryHandler listarHandler,
        IValidator<CriarTurmaCommand> validator)
    {
        _criarHandler  = criarHandler;
        _listarHandler = listarHandler;
        _validator     = validator;
    }

    /// <summary>Cria uma nova turma.</summary>
    /// <response code="201">Turma criada com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="422">Violação de regra de negócio.</response>
    [HttpPost]
    [Authorize(Roles = "Diretor")]
    [ProducesResponseType(typeof(CriarTurmaResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Criar(
        [FromBody] CriarTurmaRequest request,
        CancellationToken ct)
    {
        var command    = new CriarTurmaCommand(request.Turno, request.Periodo);
        var validation = await _validator.ValidateAsync(command, ct);

        if (!validation.IsValid)
        {
            return BadRequest(validation.ToDictionary());
        }

        var result = await _criarHandler.HandleAsync(command, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = result.TurmaId }, result);
    }

    /// <summary>Lista turmas com filtros opcionais (paginado).</summary>
    [HttpGet]
    [Authorize(Roles = "Diretor,Professor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] int?  periodo,
        [FromQuery] bool? ativo,
        [FromQuery] int   pagina       = 1,
        [FromQuery] int   tamanhoPagina = 20,
        CancellationToken ct = default)
    {
        var query  = new ListarTurmasQuery(periodo, ativo, pagina, tamanhoPagina);
        var result = await _listarHandler.HandleAsync(query, ct);
        return Ok(result);
    }

    /// <summary>Retorna uma turma por Id.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Diretor,Professor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken ct)
    {
        // TODO: implementar ObterTurmaPorIdQueryHandler
        var query = new ListarTurmasQuery(1, true, 1, 1);
        var result = await _listarHandler.HandleAsync(query, ct);
        return Ok(new { id, msg = "TODO: ObterTurmaPorIdQueryHandler" });
    }
}

/// <summary>Request body de criação de turma.</summary>
public sealed record CriarTurmaRequest(Turno Turno, int Periodo);
