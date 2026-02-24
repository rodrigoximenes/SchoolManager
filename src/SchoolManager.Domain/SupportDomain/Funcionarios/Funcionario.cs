using SchoolManager.Domain.Common.Base;
using SchoolManager.Domain.Common.Exceptions;
using SchoolManager.Domain.Common.Validations;
using SchoolManager.Domain.SupportDomain.Funcionarios.ValueObjects;

namespace SchoolManager.Domain.SupportDomain.Funcionarios;

/// <summary>
/// Agregado Funcionario — representa o Professor no contexto de RH.
/// Criado automaticamente via handler do ProfessorCriadoEvent.
/// O Id é o mesmo Guid do Professor no CoreDomain (sem FK física).
/// </summary>
public sealed class Funcionario : AggregateRoot
{
    public Guid      ProfessorId { get; private set; }
    public Endereco? Endereco    { get; private set; }

    private Funcionario(Guid professorId)
    {
        Guard.AgainstEmptyGuid(professorId, nameof(professorId),
            "ProfessorId é obrigatório para criar um Funcionario.");
        ProfessorId = professorId;
    }

    private Funcionario() { }

    /// <summary>
    /// Criado pelo handler de ProfessorCriadoEvent.
    /// O Id do Funcionario é o mesmo Id do Professor.
    /// </summary>
    public static Funcionario CriarParaProfessor(Guid professorId)
    {
        var funcionario = new Funcionario(professorId);
        // Sobrescreve o Id gerado no construtor de Entity
        // para manter o mesmo Guid do Professor
        funcionario.Id = professorId;
        return funcionario;
    }

    public void AtualizarEndereco(Endereco endereco)
    {
        Guard.Against<DomainException>(IsDeleted, "Não é possível atualizar um funcionário excluído.");
        Guard.AgainstNull(endereco, nameof(endereco));

        Endereco = endereco;
        SetDataAtualizacao();
    }

    public void Excluir()
    {
        Guard.Against<DomainException>(IsDeleted, "Funcionário já foi excluído.");
        MarcarComoExcluido();
    }
}
