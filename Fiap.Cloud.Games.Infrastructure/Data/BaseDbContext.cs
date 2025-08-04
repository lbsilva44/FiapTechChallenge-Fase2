using Audit.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Fiap.Cloud.Games.Domain.Entities;
using Fiap.Cloud.Games.Domain.ValueObjects;

namespace Fiap.Cloud.Games.Infrastructure.Data;

[AuditDbContext(ReloadDatabaseValues = true)]

public class BaseDbContext(DbContextOptions options) : AuditDbContext(options)
{
    // ---------- DbSets ----------
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<Jogo> Jogos { get; set; } = null!;
    public DbSet<Biblioteca> Bibliotecas { get; set; } = null!;
    public DbSet<Promocao> Promocoes { get; set; } = null!;
    public DbSet<PromocaoJogo> PromocaoJogos { get; set; } = null!;
    public DbSet<MovimentoCarteira> MovimentosCarteira { get; set; } = null!;

    // ---------- Fluent API ----------
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ----- Usuario com Value Objects -----
        modelBuilder.Entity<Usuario>(builder =>
        {
            builder.OwnsOne(u => u.Nome, nome =>
            {
                nome.Property(n => n.Valor)
                    .HasColumnName("Nome")
                    .IsRequired()
                    .HasMaxLength(150);
            });

            builder.OwnsOne(u => u.Email, email =>
            {
                email.Property(e => e.Valor)
                     .HasColumnName("Email")
                     .IsRequired();
            });

            builder.OwnsOne(u => u.Senha, senha =>
            {
                senha.Property(s => s.Valor)
                     .HasColumnName("SenhaHash")
                     .IsRequired();
            });

            // ---------- Carteira como Owned Type ----------
            builder.OwnsOne(u => u.Carteira, carteira =>
                        {
                            carteira.Property(c => c.Saldo)
                                        .HasColumnName("Saldo")
                                        .HasPrecision(18, 2);
                        });

            // ---------- Relação com Movimentos ----------
            builder.HasMany(u => u.Movimentos)
                               .WithOne()
                               .HasForeignKey(m => m.UsuarioId);

            builder.HasMany(u => u.Bibliotecas)            // ← nova relação
           .WithOne(b => b.Usuario)
           .HasForeignKey(b => b.UsuarioId);
        });

        // ----- Biblioteca -----
        modelBuilder.Entity<Biblioteca>(builder =>
        {
            builder.ToTable("Bibliotecas");
            builder.HasOne(b => b.Jogo)                    // permite usar ThenInclude(b => b.Jogo)
                   .WithMany()
                   .HasForeignKey(b => b.JogoId);
        });

        // ----- Jogo -----
        modelBuilder.Entity<Jogo>(builder =>
        {
            builder.Property(j => j.Preco)
                   .HasPrecision(18, 2);
        });

        // ----- Promocao -----
        modelBuilder.Entity<Promocao>(builder =>
        {
            builder.Property(p => p.DescontoPercentual)
                   .HasPrecision(5, 2);

            builder.HasMany(p => p.Jogos)
                   .WithOne(pj => pj.Promocao)
                   .HasForeignKey(pj => pj.PromocaoId);
        });

        // ----- PromocaoJogo (join table) -----
        modelBuilder.Entity<PromocaoJogo>(builder =>
        {
            builder.HasKey(pj => new { pj.PromocaoId, pj.JogoId });

            builder.HasOne(pj => pj.Promocao)
             .WithMany(p => p.Jogos)
             .HasForeignKey(pj => pj.PromocaoId);

            builder.HasOne(pj => pj.Jogo)
             .WithMany(j => j.PromocaoJogos)  // Certifique-se que Jogo também declare ICollection<PromocaoJogo>
             .HasForeignKey(pj => pj.JogoId);
        });

        // ---------- MovimentoCarteira ----------
        modelBuilder.Entity<MovimentoCarteira>(builder =>
        {
            builder.ToTable("CarteiraMovimentos");

            builder.Property(m => m.Valor).HasPrecision(18, 2);
            builder.Property(m => m.SaldoAntes).HasPrecision(18, 2);
            builder.Property(m => m.SaldoDepois).HasPrecision(18, 2);
            builder.HasIndex(m => m.JogoId);
        });
    }
}
