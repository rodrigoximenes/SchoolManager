using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SchoolManager.Infrastructure.Persistence.Contexts;

/// <summary>
/// Entidade customizada de usuário — adiciona EscolaId ao Identity.
/// </summary>
public sealed class ApplicationUser : IdentityUser
{
    public Guid EscolaId { get; set; }
}

/// <summary>
/// Entidade Escola — armazenada no banco Master.
/// </summary>
public sealed class EscolaEntity
{
    public Guid     Id                { get; set; }
    public string   Nome              { get; set; } = null!;
    public string   CNPJ              { get; set; } = null!;
    public string   ConnectionString  { get; set; } = null!;
    public bool     Ativo             { get; set; }
    public bool     TermosAceitos     { get; set; }
    public DateTime? DataAceiteTermos { get; set; }
    public DateTime DataCriacao       { get; set; }
    public DateTime? DataAtualizacao  { get; set; }
}

/// <summary>
/// Entidade RefreshToken — armazenada no banco Master.
/// </summary>
public sealed class RefreshTokenEntity
{
    public Guid     Id             { get; set; }
    public string   UserId         { get; set; } = null!;
    public string   Token          { get; set; } = null!;
    public DateTime DataExpiracao  { get; set; }
    public bool     IsRevoked      { get; set; }
    public DateTime DataCriacao    { get; set; }

    public ApplicationUser? Usuario { get; set; }
}

/// <summary>
/// DbContext do banco Master — Identity + Escolas + RefreshTokens.
/// Compartilhado por todos os tenants.
/// </summary>
public sealed class MasterDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<EscolaEntity>       Escolas       => Set<EscolaEntity>();
    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();

    public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<EscolaEntity>(e =>
        {
            e.ToTable("Escolas");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).HasMaxLength(200).IsRequired();
            e.Property(x => x.CNPJ).HasMaxLength(18).IsRequired();
            e.Property(x => x.ConnectionString).HasMaxLength(500).IsRequired();
            e.HasIndex(x => x.CNPJ).IsUnique();
            e.HasIndex(x => x.Ativo);
        });

        builder.Entity<RefreshTokenEntity>(e =>
        {
            e.ToTable("RefreshTokens");
            e.HasKey(x => x.Id);
            e.Property(x => x.Token).HasMaxLength(500).IsRequired();
            e.HasIndex(x => x.Token).IsUnique();
            e.HasIndex(x => x.UserId);
            e.HasOne(x => x.Usuario)
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
