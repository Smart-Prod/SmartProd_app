using Microsoft.EntityFrameworkCore;
using SmartProd.API.Server.Models;

namespace SmartProd.API.Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Materiais> Materiais { get; set; }
        public DbSet<MateriaisItems> MateriaisItems { get; set; }
        public DbSet<OrdemProducao> OrdensProducao { get; set; }
        public DbSet<Movimentacao> Movimentacoes { get; set; }
        public DbSet<NotaFiscal> NotasFiscais { get; set; }
        public DbSet<NotaFiscalItem> NotaFiscalItems { get; set; }
        public DbSet<Estoque> Estoque { get; set; }
        public DbSet<Vendas> Vendas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Usuario ---
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();
            });

            // --- Produto ---
            modelBuilder.Entity<Produto>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.Code).IsUnique();
                entity.Property(p => p.Name).IsRequired();

                entity.HasOne(p => p.Usuario)
                    .WithMany(u => u.Produtos)
                    .HasForeignKey(p => p.UsuarioId);
            });

            // --- Materiais (BOM) - 1:1 com Produto ---
            modelBuilder.Entity<Produto>()
                .HasOne(p => p.Bom)
                .WithOne(m => m.Produto)
                .HasForeignKey<Materiais>(m => m.ProdutoId);

            // --- MateriaisItems ---
            modelBuilder.Entity<MateriaisItems>(entity =>
            {
                entity.HasKey(mi => mi.Id);
                entity.HasOne(mi => mi.Materiais)
                    .WithMany(m => m.Materials)
                    .HasForeignKey(mi => mi.MateriaisId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(mi => mi.Produtos)
                    .WithMany(p => p.MaterialsUsed)
                    .HasForeignKey(mi => mi.ProdutosId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // --- OrdemProducao ---
            modelBuilder.Entity<OrdemProducao>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.HasOne(o => o.Produto)
                    .WithMany(p => p.ProductionOrders)
                    .HasForeignKey(o => o.ProductId);

                entity.HasOne(o => o.Usuario)
                    .WithMany(u => u.OrdemProducao)
                    .HasForeignKey(o => o.UsuarioId);
            });

            // --- Movimentacao ---
            modelBuilder.Entity<Movimentacao>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.HasOne(m => m.Produto)
                    .WithMany(p => p.StockMovements)
                    .HasForeignKey(m => m.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // --- NotaFiscal ---
            modelBuilder.Entity<NotaFiscal>(entity =>
            {
                entity.HasKey(nf => nf.Id);
                entity.HasIndex(nf => nf.Number).IsUnique();
                entity.Property(nf => nf.Type).IsRequired();
                entity.Property(nf => nf.Status).IsRequired();

                entity.HasOne(nf => nf.Usuario)
                    .WithMany(u => u.NotaFiscal)
                    .HasForeignKey(nf => nf.UsuarioId);

                entity.HasMany(nf => nf.Items)
                    .WithOne(i => i.NotaFiscal)
                    .HasForeignKey(i => i.NotaFiscalId);
            });

            // --- NotaFiscalItem ---
            modelBuilder.Entity<NotaFiscalItem>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.HasOne(i => i.Produto)
                    .WithMany(p => p.InvoiceItems)
                    .HasForeignKey(i => i.ProductId);
            });

            // --- Estoque ---
            modelBuilder.Entity<Estoque>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Produto)
                    .WithMany()
                    .HasForeignKey(e => e.ProductId);
            });

            // --- Vendas ---
            modelBuilder.Entity<Vendas>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.HasOne(v => v.Produto)
                    .WithMany()
                    .HasForeignKey(v => v.ProductId);
            });
        }
    }
}
