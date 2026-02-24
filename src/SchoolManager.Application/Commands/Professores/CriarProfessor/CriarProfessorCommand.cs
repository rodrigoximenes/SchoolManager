using FluentValidation;
using SchoolManager.Application.Abstractions.Persistence;
using SchoolManager.Domain.CoreDomain.Professores;

namespace SchoolManager.Application.Commands.Professores.CriarProfessor;

public sealed class CriarProfessorCommand
{
    public string PrimeiroNome { get; }
    public string Sobrenome    { get; }

    public CriarProfessorCommand(string primeiroNome, string sobrenome)
    {
        PrimeiroNome = primeiroNome;
        Sobrenome    = sobrenome;
    }
}

public sealed class CriarProfessorResultDto
{
    public Guid   ProfessorId  { get; }
    public string NomeCompleto { get; }

    private CriarProfessorResultDto(Guid id, string nomeCompleto)
    {
        ProfessorId  = id;
        NomeCompleto = nomeCompleto;
    }

    public static CriarProfessorResultDto FromDomain(Professor p)
        => new(p.Id, p.Nome.NomeCompleto);
}

public sealed class CriarProfessorCommandHandler
{
    private readonly IProfessorRepository _repo;

    public CriarProfessorCommandHandler(IProfessorRepository repo)
        => _repo = repo;

    public async Task<CriarProfessorResultDto> HandleAsync(
        CriarProfessorCommand command,
        CancellationToken ct = default)
    {
        var professor = Professor.Criar(command.PrimeiroNome, command.Sobrenome);
        await _repo.AdicionarAsync(professor, ct);
        await _repo.SalvarAlteracoesAsync(ct);
        // ProfessorCriadoEvent é despachado pelo DbContext após SaveChanges
        // → cria Funcionario automaticamente no SupportDomain
        return CriarProfessorResultDto.FromDomain(professor);
    }
}

public sealed class CriarProfessorRequestValidator : AbstractValidator<CriarProfessorCommand>
{
    public CriarProfessorRequestValidator()
    {
        RuleFor(x => x.PrimeiroNome)
            .NotEmpty().WithMessage("Primeiro nome é obrigatório.")
            .MaximumLength(100).WithMessage("Primeiro nome não pode ter mais de 100 caracteres.");

        RuleFor(x => x.Sobrenome)
            .NotEmpty().WithMessage("Sobrenome é obrigatório.")
            .MaximumLength(200).WithMessage("Sobrenome não pode ter mais de 200 caracteres.");
    }
}
