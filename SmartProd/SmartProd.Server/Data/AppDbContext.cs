using Microsoft.EntityFrameworkCore;
using SmartProd.API.Server.Models;

namespace SmartProd.API.Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Entidades principais
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Movimentacao> Movimentacoes { get; set; }
        public DbSet<NotaFiscal> NotasFiscais { get; set; }
        public DbSet<NotaFiscalItem> NotaFiscalItems { get; set; }
        public DbSet<OrdemProducao> OrdensProducao { get; set; }
        public DbSet<Materiais> Materiais { get; set; }
        public DbSet<Estoque> Estoque { get; set; }
        public DbSet<Vendas> Vendas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Produto
            modelBuilder.Entity<Produto>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.Code).IsUnique();
                entity.Property(p => p.Name).IsRequired();
                // Relacionamentos, se aplicável
            });

            // Movimentação
            modelBuilder.Entity<Movimentacao>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.HasOne(m => m.Produto)
                    .WithMany(p => p.StockMovements)
                    .HasForeignKey(m => m.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Nota Fiscal
            modelBuilder.Entity<NotaFiscal>(entity =>
            {
                entity.HasKey(nf => nf.Id);
                entity.HasIndex(nf => nf.Number).IsUnique();
                entity.Property(nf => nf.Type).IsRequired();
                entity.Property(nf => nf.Status).IsRequired();
                entity.HasMany(nf => nf.Items)
                      .WithOne(i => i.NotaFiscal)
                      .HasForeignKey(i => i.NotaFiscalId);
            });

            // Nota Fiscal Item
            modelBuilder.Entity<NotaFiscalItem>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.HasOne(i => i.Produto)
                      .WithMany(p => p.InvoiceItems)
                      .HasForeignKey(i => i.ProductId);
            });

            // Outros relacionamentos podem ser configurados aqui
        }
    }
}
