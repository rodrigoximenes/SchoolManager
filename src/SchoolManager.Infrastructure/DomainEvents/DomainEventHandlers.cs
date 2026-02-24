using SchoolManager.Application.Abstractions.Persistence;
using SchoolManager.Domain.Common.Interfaces;
using SchoolManager.Domain.CoreDomain.Alunos.Events;
using SchoolManager.Domain.CoreDomain.Professores.Events;
using SchoolManager.Domain.SupportDomain.Funcionarios;
using SchoolManager.Domain.SupportDomain.Matriculas;

namespace SchoolManager.Infrastructure.DomainEvents;

/// <summary>
/// Quando um Professor é criado no CoreDomain,
/// cria automaticamente o Funcionario correspondente no SupportDomain.
/// O Id do Funcionario é o mesmo do Professor (mesmo Guid entre BCs).
/// </summary>
public sealed class ProfessorCriadoEventHandler
    : IDomainEventHandler<ProfessorCriadoEvent>
{
    private readonly IFuncionarioRepository _repo;

    public ProfessorCriadoEventHandler(IFuncionarioRepository repo)
        => _repo = repo;

    public async Task HandleAsync(
        ProfessorCriadoEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var funcionario = Funcionario.CriarParaProfessor(domainEvent.ProfessorId);
        await _repo.AdicionarAsync(funcionario, cancellationToken);
        await _repo.SalvarAlteracoesAsync(cancellationToken);
    }
}

/// <summary>
/// Quando um Professor é excluído no CoreDomain,
/// aplica soft delete no Funcionario no SupportDomain.
/// </summary>
public sealed class ProfessorExcluidoEventHandler
    : IDomainEventHandler<ProfessorExcluidoEvent>
{
    private readonly IFuncionarioRepository _repo;

    public ProfessorExcluidoEventHandler(IFuncionarioRepository repo)
        => _repo = repo;

    public async Task HandleAsync(
        ProfessorExcluidoEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var funcionario = await _repo.ObterPorProfessorIdAsync(domainEvent.ProfessorId, cancellationToken);
        if (funcionario is null) return;

        funcionario.Excluir();
        await _repo.SalvarAlteracoesAsync(cancellationToken);
    }
}

/// <summary>
/// Quando um Aluno é criado no CoreDomain,
/// cria automaticamente a Matricula no SupportDomain.
/// </summary>
public sealed class AlunoCriadoEventHandler
    : IDomainEventHandler<AlunoCriadoEvent>
{
    private readonly IMatriculaRepository _repo;

    public AlunoCriadoEventHandler(IMatriculaRepository repo)
        => _repo = repo;

    public async Task HandleAsync(
        AlunoCriadoEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var matricula = Matricula.CriarParaAluno(domainEvent.AlunoId);
        await _repo.AdicionarAsync(matricula, cancellationToken);
        await _repo.SalvarAlteracoesAsync(cancellationToken);
    }
}
