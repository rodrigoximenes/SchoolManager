using SchoolManager.Domain.Common.Base;
using SchoolManager.Domain.Common.Validations;

namespace SchoolManager.Domain.CoreDomain.Alunos.ValueObjects;

/// <summary>
/// Value Object que representa uma nota lançada para um aluno em uma disciplina.
/// Mapeado como OwnsMany no EF Core — tabela Notas.
/// </summary>
public sealed class Nota : ValueObject
{
    public Guid     Id           { get; }
    public Guid     DisciplinaId { get; }
    public decimal  Valor        { get; }
    public DateOnly Data         { get; }

    private Nota(Guid disciplinaId, decimal valor, DateOnly data)
    {
        Guard.AgainstEmptyGuid(disciplinaId, nameof(disciplinaId));
        Guard.Against<ArgumentException>(valor < 0 || valor > 10,
            $"Nota deve estar entre 0 e 10. Valor informado: {valor}.");

        Id           = Guid.NewGuid();
        DisciplinaId = disciplinaId;
        Valor        = Math.Round(valor, 2);
        Data         = data;
    }

    private Nota() { }

    public static Nota Criar(Guid disciplinaId, decimal valor, DateOnly data)
        => new(disciplinaId, valor, data);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return DisciplinaId;
        yield return Valor;
        yield return Data;
    }
}
