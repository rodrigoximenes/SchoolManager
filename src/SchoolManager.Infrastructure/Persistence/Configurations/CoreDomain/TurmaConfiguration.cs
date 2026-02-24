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

        builder.Property(t => t.Periodo)
            .IsRequired();

        builder.Property(t => t.Ativo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(t => t.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.DataCriacao).IsRequired();
        builder.Property(t => t.DataAtualizacao);
        builder.Property(t => t.DataExclusao);

        // Índices
        builder.HasIndex(t => t.Periodo);
        builder.HasIndex(t => t.Ativo);
        builder.HasIndex(t => t.IsDeleted);

        // Coleções de Guid mapeadas como owned tables
        // Turma.Professores e Turma.Alunos são List<Guid>
        // Mapeados como tabelas separadas TurmaProfessores e TurmaAlunos
        builder.OwnsMany(t => t.Professores, nav =>
        {
            nav.ToTable("TurmaProfessores");
            nav.WithOwner().HasForeignKey("TurmaId");
            nav.Property<Guid>("Value").HasColumnName("ProfessorId");
            nav.HasKey("TurmaId", "Value");
        });

        builder.OwnsMany(t => t.Alunos, nav =>
        {
            nav.ToTable("TurmaAlunos");
            nav.WithOwner().HasForeignKey("TurmaId");
            nav.Property<Guid>("Value").HasColumnName("AlunoId");
            nav.HasKey("TurmaId", "Value");
        });
    }
}
