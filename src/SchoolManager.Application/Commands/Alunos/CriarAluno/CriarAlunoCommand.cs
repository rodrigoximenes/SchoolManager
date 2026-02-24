using FluentValidation;
using SchoolManager.Application.Abstractions.Persistence;
using SchoolManager.Domain.CoreDomain.Alunos;

namespace SchoolManager.Application.Commands.Alunos.CriarAluno;

public sealed class CriarAlunoCommand
{
    public string    Nome           { get; }
    public DateOnly? DataNascimento { get; }

    public CriarAlunoCommand(string nome, DateOnly? dataNascimento)
    {
        Nome           = nome;
        DataNascimento = dataNascimento;
    }
}

public sealed class CriarAlunoResultDto
{
    public Guid    AlunoId        { get; }
    public string  Nome           { get; }
    public DateOnly? DataNascimento { get; }

    private CriarAlunoResultDto(Guid id, string nome, DateOnly? dataNascimento)
    {
        AlunoId        = id;
        Nome           = nome;
        DataNascimento = dataNascimento;
    }

    public static CriarAlunoResultDto FromDomain(Aluno a)
        => new(a.Id, a.Nome, a.DataNascimento);
}

public sealed class CriarAlunoCommandHandler
{
    private readonly IAlunoRepository _repo;

    public CriarAlunoCommandHandler(IAlunoRepository repo)
        => _repo = repo;

    public async Task<CriarAlunoResultDto> HandleAsync(
        CriarAlunoCommand command,
        CancellationToken ct = default)
    {
        var aluno = Aluno.Criar(command.Nome, command.DataNascimento);
        await _repo.AdicionarAsync(aluno, ct);
        await _repo.SalvarAlteracoesAsync(ct);
        // AlunoCriadoEvent é despachado pelo DbContext
        // → cria Matricula automaticamente no SupportDomain
        return CriarAlunoResultDto.FromDomain(aluno);
    }
}

public sealed class CriarAlunoRequestValidator : AbstractValidator<CriarAlunoCommand>
{
    public CriarAlunoRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome do aluno é obrigatório.")
            .MaximumLength(300).WithMessage("Nome não pode ter mais de 300 caracteres.");

        RuleFor(x => x.DataNascimento)
            .LessThan(DateOnly.FromDateTime(DateTime.Today))
            .When(x => x.DataNascimento.HasValue)
            .WithMessage("Data de nascimento deve ser anterior à data de hoje.");
    }
}
