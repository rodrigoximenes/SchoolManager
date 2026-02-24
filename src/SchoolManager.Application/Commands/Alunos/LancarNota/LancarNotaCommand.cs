using FluentValidation;
using SchoolManager.Application.Abstractions.Persistence;
using SchoolManager.Domain.Common.Exceptions;

namespace SchoolManager.Application.Commands.Alunos.LancarNota;

public sealed class LancarNotaCommand
{
    public Guid     AlunoId      { get; }
    public Guid     DisciplinaId { get; }
    public decimal  Valor        { get; }
    public DateOnly Data         { get; }

    public LancarNotaCommand(Guid alunoId, Guid disciplinaId, decimal valor, DateOnly data)
    {
        AlunoId      = alunoId;
        DisciplinaId = disciplinaId;
        Valor        = valor;
        Data         = data;
    }
}

public sealed class LancarNotaResultDto
{
    public Guid    NotaId      { get; }
    public decimal Valor       { get; }
    public DateOnly Data       { get; }

    public LancarNotaResultDto(Guid notaId, decimal valor, DateOnly data)
    {
        NotaId = notaId;
        Valor  = valor;
        Data   = data;
    }
}

public sealed class LancarNotaCommandHandler
{
    private readonly IAlunoRepository _repo;

    public LancarNotaCommandHandler(IAlunoRepository repo) => _repo = repo;

    public async Task<LancarNotaResultDto> HandleAsync(
        LancarNotaCommand command,
        CancellationToken ct = default)
    {
        var aluno = await _repo.ObterPorIdAsync(command.AlunoId, ct)
            ?? throw new DomainException($"Aluno '{command.AlunoId}' não encontrado.");

        aluno.LancarNota(command.DisciplinaId, command.Valor, command.Data);
        await _repo.SalvarAlteracoesAsync(ct);

        var nota = aluno.Notas.Last();
        return new LancarNotaResultDto(nota.Id, nota.Valor, nota.Data);
    }
}

public sealed class LancarNotaRequestValidator : AbstractValidator<LancarNotaCommand>
{
    public LancarNotaRequestValidator()
    {
        RuleFor(x => x.AlunoId)
            .NotEmpty().WithMessage("AlunoId é obrigatório.");

        RuleFor(x => x.DisciplinaId)
            .NotEmpty().WithMessage("DisciplinaId é obrigatório.");

        RuleFor(x => x.Valor)
            .InclusiveBetween(0, 10)
            .WithMessage("Nota deve estar entre 0 e 10.");

        RuleFor(x => x.Data)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Data da nota não pode ser futura.");
    }
}
