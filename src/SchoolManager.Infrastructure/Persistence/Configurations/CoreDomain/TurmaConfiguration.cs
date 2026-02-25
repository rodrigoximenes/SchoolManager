using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManager.Domain.CoreDomain.Turmas;

namespace SchoolManager.Infrastructure.Persistence.Configurations.CoreDomain;

public sealed class TurmaConfiguration : IEntityTypeConfiguration<Turma>
{
    public void Configure(EntityTypeBuilder<Turma> builder)
    {
        builder.ToTable("Turmas");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Turno)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.Periodo).IsRequired();
        builder.Property(t => t.Ativo).IsRequired().HasDefaultValue(true);
        builder.Property(t => t.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(t => t.DataCriacao).IsRequired();
        builder.Property(t => t.DataAtualizacao);
        builder.Property(t => t.DataExclusao);

        builder.HasIndex(t => t.Periodo);
        builder.HasIndex(t => t.Ativo);
        builder.HasIndex(t => t.IsDeleted);

        // TurmaProfessor — entidade de junção owned pelo agregado
        builder.OwnsMany<TurmaProfessor>("_professores", nav =>
        {
            nav.ToTable("TurmaProfessores");
            nav.WithOwner().HasForeignKey(tp => tp.TurmaId);
            nav.HasKey(tp => new { tp.TurmaId, tp.ProfessorId });
            nav.Property(tp => tp.ProfessorId).IsRequired();
            nav.Property(tp => tp.DataVinculo).IsRequired();
            nav.HasIndex(tp => tp.ProfessorId);
        });

        // TurmaAluno — entidade de junção owned pelo agregado
        builder.OwnsMany<TurmaAluno>("_alunos", nav =>
        {
            nav.ToTable("TurmaAlunos");
            nav.WithOwner().HasForeignKey(ta => ta.TurmaId);
            nav.HasKey(ta => new { ta.TurmaId, ta.AlunoId });
            nav.Property(ta => ta.AlunoId).IsRequired();
            nav.Property(ta => ta.DataVinculo).IsRequired();
            nav.HasIndex(ta => ta.AlunoId);
        });
    }
}
