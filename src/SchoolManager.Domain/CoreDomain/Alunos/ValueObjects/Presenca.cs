using SchoolManager.Domain.Common.Base;
using SchoolManager.Domain.Common.Validations;

namespace SchoolManager.Domain.CoreDomain.Alunos.ValueObjects;

/// <summary>
/// Value Object que representa o registro de presença de um aluno.
/// Mapeado como OwnsMany no EF Core — tabela Presencas.
/// Unique: (AlunoId, DisciplinaId, Data) — configurado no EF Core.
/// </summary>
public sealed class Presenca : ValueObject
{
    public Guid     Id           { get; }
    public Guid     DisciplinaId { get; }
    public DateOnly Data         { get; }
    public bool     Presente     { get; }

    private Presenca(Guid disciplinaId, DateOnly data, bool presente)
    {
        Guard.AgainstEmptyGuid(disciplinaId, nameof(disciplinaId));

        Id           = Guid.NewGuid();
        DisciplinaId = disciplinaId;
        Data         = data;
        Presente     = presente;
    }

    private Presenca() { }

    public static Presenca Criar(Guid disciplinaId, DateOnly data, bool presente)
        => new(disciplinaId, data, presente);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return DisciplinaId;
        yield return Data;
    }
}
