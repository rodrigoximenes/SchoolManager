using SchoolManager.Domain.Common.Base;
using SchoolManager.Domain.Common.Exceptions;
using SchoolManager.Domain.Common.Validations;
using SchoolManager.Domain.SupportDomain.Funcionarios.ValueObjects;
using SchoolManager.Domain.SupportDomain.Matriculas.Enums;
using SchoolManager.Domain.SupportDomain.Matriculas.Events;

namespace SchoolManager.Domain.SupportDomain.Matriculas;

/// <summary>
/// Agregado Matricula — contexto financeiro/RH do aluno.
/// Criado automaticamente via handler do AlunoCriadoEvent.
/// AlunoId referencia Aluno no CoreDomain (sem FK física entre BCs).
/// </summary>
public sealed class Matricula : AggregateRoot
{
    public Guid            AlunoId             { get; private set; }
    public StatusMatricula StatusMatricula     { get; private set; }
    public bool            MensalidadeEmDia    { get; private set; }
    public DateTime        DataMatricula       { get; private set; }
    public DateTime?       DataCancelamento    { get; private set; }
    public string?         MotivoCancelamento  { get; private set; }
    public Endereco?       EnderecoAluno       { get; private set; }

    private Matricula(Guid alunoId)
    {
        Guard.AgainstEmptyGuid(alunoId, nameof(alunoId));

        AlunoId          = alunoId;
        StatusMatricula  = StatusMatricula.Ativa;
        MensalidadeEmDia = true;
        DataMatricula    = DateTime.UtcNow;

        AddDomainEvent(new MatriculaCriadaEvent(Id, alunoId));
    }

    private Matricula() { }

    /// <summary>Criado pelo handler de AlunoCriadoEvent.</summary>
    public static Matricula CriarParaAluno(Guid alunoId) => new(alunoId);

    public void Cancelar(string motivo)
    {
        Guard.Against<DomainException>(IsDeleted,
            "Não é possível cancelar uma matrícula excluída.");
        Guard.Against<DomainException>(StatusMatricula == StatusMatricula.Cancelada,
            "Matrícula já está cancelada.");
        Guard.AgainstNullOrWhiteSpace(motivo, nameof(motivo), "Motivo do cancelamento é obrigatório.");

        StatusMatricula    = StatusMatricula.Cancelada;
        DataCancelamento   = DateTime.UtcNow;
        MotivoCancelamento = motivo.Trim();
        SetDataAtualizacao();

        AddDomainEvent(new MatriculaCanceladaEvent(Id, AlunoId, motivo));
    }

    public void Suspender()
    {
        Guard.Against<DomainException>(IsDeleted, "Não é possível suspender uma matrícula excluída.");
        Guard.Against<DomainException>(StatusMatricula != StatusMatricula.Ativa,
            "Somente matrículas ativas podem ser suspensas.");

        StatusMatricula = StatusMatricula.Suspensa;
        SetDataAtualizacao();
        AddDomainEvent(new MatriculaSuspensaEvent(Id, AlunoId));
    }

    public void Ativar()
    {
        Guard.Against<DomainException>(IsDeleted, "Não é possível ativar uma matrícula excluída.");
        Guard.Against<DomainException>(StatusMatricula == StatusMatricula.Cancelada,
            "Matrícula cancelada não pode ser reativada. Crie uma nova matrícula.");

        StatusMatricula = StatusMatricula.Ativa;
        SetDataAtualizacao();
    }

    public void MarcarMensalidadeEmDia()
    {
        MensalidadeEmDia = true;
        SetDataAtualizacao();
    }

    public void MarcarMensalidadeAtrasada()
    {
        MensalidadeEmDia = false;
        SetDataAtualizacao();
    }

    public void AtualizarEndereco(Endereco endereco)
    {
        Guard.AgainstNull(endereco, nameof(endereco));
        EnderecoAluno = endereco;
        SetDataAtualizacao();
    }
}
