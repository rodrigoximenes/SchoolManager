using SchoolManager.Application.Common.Pagination;
using SchoolManager.Domain.CoreDomain.Alunos;
using SchoolManager.Domain.CoreDomain.Mensagens;
using SchoolManager.Domain.CoreDomain.Professores;
using SchoolManager.Domain.CoreDomain.Turmas;
using SchoolManager.Domain.SupportDomain.Funcionarios;
using SchoolManager.Domain.SupportDomain.Matriculas;

namespace SchoolManager.Application.Abstractions.Persistence;

public interface ITurmaRepository
{
    Task<Turma?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<Turma>> ListarAsync(int? periodo, bool? ativo, PagedQuery paginacao, CancellationToken ct = default);
    Task AdicionarAsync(Turma turma, CancellationToken ct = default);
    Task<bool> ExisteAsync(Guid id, CancellationToken ct = default);
    Task SalvarAlteracoesAsync(CancellationToken ct = default);
}

public interface IProfessorRepository
{
    Task<Professor?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<Professor>> ListarAsync(string? busca, PagedQuery paginacao, CancellationToken ct = default);
    Task AdicionarAsync(Professor professor, CancellationToken ct = default);
    Task SalvarAlteracoesAsync(CancellationToken ct = default);
}

public interface IAlunoRepository
{
    Task<Aluno?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<Aluno>> ListarPorTurmaAsync(Guid turmaId, PagedQuery paginacao, CancellationToken ct = default);
    Task<PagedResult<Aluno>> ListarAsync(string? busca, PagedQuery paginacao, CancellationToken ct = default);
    Task AdicionarAsync(Aluno aluno, CancellationToken ct = default);
    Task SalvarAlteracoesAsync(CancellationToken ct = default);
}

public interface IMensagemRepository
{
    Task<Mensagem?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<Mensagem>> ListarRecebidasAsync(Guid destinatarioId, PagedQuery paginacao, CancellationToken ct = default);
    Task<PagedResult<Mensagem>> ListarEnviadasAsync(Guid remetenteId, PagedQuery paginacao, CancellationToken ct = default);
    Task AdicionarAsync(Mensagem mensagem, CancellationToken ct = default);
    Task SalvarAlteracoesAsync(CancellationToken ct = default);
}

public interface IFuncionarioRepository
{
    Task<Funcionario?> ObterPorProfessorIdAsync(Guid professorId, CancellationToken ct = default);
    Task AdicionarAsync(Funcionario funcionario, CancellationToken ct = default);
    Task SalvarAlteracoesAsync(CancellationToken ct = default);
}

public interface IMatriculaRepository
{
    Task<Matricula?> ObterPorAlunoIdAsync(Guid alunoId, CancellationToken ct = default);
    Task<PagedResult<Matricula>> ListarAsync(string? status, PagedQuery paginacao, CancellationToken ct = default);
    Task AdicionarAsync(Matricula matricula, CancellationToken ct = default);
    Task SalvarAlteracoesAsync(CancellationToken ct = default);
}
