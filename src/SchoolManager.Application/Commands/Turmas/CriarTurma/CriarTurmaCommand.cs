using FluentValidation;
using SchoolManager.Application.Abstractions.Persistence;
using SchoolManager.Domain.CoreDomain.Turmas;
using SchoolManager.Domain.CoreDomain.Turmas.Enums;

namespace SchoolManager.Application.Commands.Turmas.CriarTurma;

// ── Command ───────────────────────────────────────────────────────────────────

public sealed class CriarTurmaCommand
{
    public Turno Turno   { get; }
    public int   Periodo { get; }

    public CriarTurmaCommand(Turno turno, int periodo)
    {
        Turno   = turno;
        Periodo = periodo;
    }
}

// ── Result DTO ────────────────────────────────────────────────────────────────

public sealed class CriarTurmaResultDto
{
    public Guid   TurmaId { get; }
    public string Turno   { get; }
    public int    Periodo { get; }
    public bool   Ativo   { get; }

    private CriarTurmaResultDto(Guid id, string turno, int periodo, bool ativo)
    {
        TurmaId = id;
        Turno   = turno;
        Periodo = periodo;
        Ativo   = ativo;
    }

    public static CriarTurmaResultDto FromDomain(Turma t)
        => new(t.Id, t.Turno.ToString(), t.Periodo, t.Ativo);
}

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class CriarTurmaCommandHandler
{
    private readonly ITurmaRepository _repo;

    public CriarTurmaCommandHandler(ITurmaRepository repo)
        => _repo = repo;

    public async Task<CriarTurmaResultDto> HandleAsync(
        CriarTurmaCommand command,
        CancellationToken ct = default)
    {
        var turma = Turma.Criar(command.Turno, command.Periodo);
        await _repo.AdicionarAsync(turma, ct);
        await _repo.SalvarAlteracoesAsync(ct);
        return CriarTurmaResultDto.FromDomain(turma);
    }
}

// ── Validator ─────────────────────────────────────────────────────────────────

public sealed class CriarTurmaRequestValidator : AbstractValidator<CriarTurmaCommand>
{
    public CriarTurmaRequestValidator()
    {
        RuleFor(x => x.Turno)
            .IsInEnum()
            .WithMessage("Turno inválido. Use 0 (Manhã) ou 1 (Tarde).");

        RuleFor(x => x.Periodo)
            .InclusiveBetween(2000, 2100)
            .WithMessage("Período deve ser um ano letivo válido (entre 2000 e 2100).");
    }
}
