using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolManager.Domain.CoreDomain.Alunos;
using SchoolManager.Domain.CoreDomain.Professores;

namespace SchoolManager.Infrastructure.Persistence.Configurations.CoreDomain;

public sealed class ProfessorConfiguration : IEntityTypeConfiguration<Professor>
{
    public void Configure(EntityTypeBuilder<Professor> builder)
    {
        builder.ToTable("Professores");
        builder.HasKey(p => p.Id);

        // Value Object NomePessoa → OwnedOne → colunas NomePessoa_PrimeiroNome, NomePessoa_Sobrenome
        builder.OwnsOne(p => p.Nome, nome =>
        {
            nome.Property(n => n.PrimeiroNome)
                .HasColumnName("NomePessoa_PrimeiroNome")
                .HasMaxLength(100)
                .IsRequired();

            nome.Property(n => n.Sobrenome)
                .HasColumnName("NomePessoa_Sobrenome")
                .HasMaxLength(200)
                .IsRequired();
        });

        // Disciplinas → OwnsMany → tabela Disciplinas
        builder.OwnsMany(p => p.Disciplinas, disc =>
        {
            disc.ToTable("Disciplinas");
            disc.WithOwner().HasForeignKey("ProfessorId");
            disc.HasKey(d => d.Id);
            disc.Property(d => d.Nome).HasMaxLength(150).IsRequired();
            disc.HasIndex("ProfessorId");
            disc.HasIndex(d => new { d.Nome }).IsUnique(); // unique por professor
        });

        builder.Property(p => p.IsDeleted).HasDefaultValue(false);
        builder.Property(p => p.DataCriacao).IsRequired();
        builder.Property(p => p.DataAtualizacao);
        builder.Property(p => p.DataExclusao);

        builder.HasIndex("NomePessoa_Sobrenome");
        builder.HasIndex(p => p.IsDeleted);
    }
}

public sealed class AlunoConfiguration : IEntityTypeConfiguration<Aluno>
{
    public void Configure(EntityTypeBuilder<Aluno> builder)
    {
        builder.ToTable("Alunos");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Nome).HasMaxLength(300).IsRequired();
        builder.Property(a => a.DataNascimento);
        builder.Property(a => a.IsDeleted).HasDefaultValue(false);
        builder.Property(a => a.DataCriacao).IsRequired();
        builder.Property(a => a.DataAtualizacao);
        builder.Property(a => a.DataExclusao);

        // AlunoResponsavel → entidade filho
        builder.HasMany(a => a.Responsaveis)
            .WithOne()
            .HasForeignKey(r => r.AlunoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Notas → OwnsMany
        builder.OwnsMany(a => a.Notas, nota =>
        {
            nota.ToTable("Notas");
            nota.WithOwner().HasForeignKey("AlunoId");
            nota.HasKey(n => n.Id);
            nota.Property(n => n.DisciplinaId).IsRequired();
            nota.Property(n => n.Valor).HasPrecision(5, 2).IsRequired();
            nota.Property(n => n.Data).IsRequired();
            nota.HasIndex(n => n.DisciplinaId);
            nota.HasIndex("AlunoId");
        });

        // Presencas → OwnsMany
        builder.OwnsMany(a => a.Presencas, pres =>
        {
            pres.ToTable("Presencas");
            pres.WithOwner().HasForeignKey("AlunoId");
            pres.HasKey(p => p.Id);
            pres.Property(p => p.DisciplinaId).IsRequired();
            pres.Property(p => p.Data).IsRequired();
            pres.Property(p => p.Presente).IsRequired();
            pres.HasIndex(new[] { "AlunoId", nameof(Domain.CoreDomain.Alunos.ValueObjects.Presenca.DisciplinaId), nameof(Domain.CoreDomain.Alunos.ValueObjects.Presenca.Data) }).IsUnique();
        });

        builder.HasIndex(a => a.Nome);
        builder.HasIndex(a => a.IsDeleted);
    }
}

public sealed class AlunoResponsavelConfiguration : IEntityTypeConfiguration<AlunoResponsavel>
{
    public void Configure(EntityTypeBuilder<AlunoResponsavel> builder)
    {
        builder.ToTable("AlunoResponsaveis");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Nome).HasMaxLength(300).IsRequired();
        builder.Property(r => r.Parentesco).HasMaxLength(100);
        builder.Property(r => r.Telefone).HasMaxLength(20);
        builder.HasIndex(r => r.AlunoId);
    }
}
