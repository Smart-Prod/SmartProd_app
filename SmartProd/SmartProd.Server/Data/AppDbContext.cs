using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Security.Claims;
using System.Text.Json;
using SmartProd.API.Server.Models;
using SmartProd.API.Server.Services;

namespace SmartProd.API.Server.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;
        private readonly ITenantContext? _tenantContext;

        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            ITenantContext? tenantContext = null,
            IHttpContextAccessor? httpContextAccessor = null) : base(options)
        {
            _tenantContext = tenantContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public int CurrentTenantId => _tenantContext?.TenantId ?? 1;

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<AuditEvent> AuditEvents { get; set; }
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
        public DbSet<WorkCenter> WorkCenters { get; set; }
        public DbSet<Machine> Machines { get; set; }
        public DbSet<ProductionOperation> ProductionOperations { get; set; }
        public DbSet<ProductionEvent> ProductionEvents { get; set; }
        public DbSet<Lot> Lots { get; set; }
        public DbSet<LotGenealogy> LotGenealogy { get; set; }

        public override int SaveChanges() => SaveChanges(acceptAllChangesOnSuccess: true);

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            AssignTenantToNewEntities();
            var pendingAudit = CaptureAuditableChanges();
            using var transaction = Database.IsRelational() && Database.CurrentTransaction is null
                ? Database.BeginTransaction()
                : null;

            var affectedRows = base.SaveChanges(acceptAllChangesOnSuccess: false);
            PersistAuditEvents(pendingAudit, acceptAllChangesOnSuccess);
            transaction?.Commit();
            return affectedRows;
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken);

        public override async Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default)
        {
            AssignTenantToNewEntities();
            var pendingAudit = CaptureAuditableChanges();
            await using var transaction = Database.IsRelational() && Database.CurrentTransaction is null
                ? await Database.BeginTransactionAsync(cancellationToken)
                : null;

            var affectedRows = await base.SaveChangesAsync(acceptAllChangesOnSuccess: false, cancellationToken);
            await PersistAuditEventsAsync(pendingAudit, acceptAllChangesOnSuccess, cancellationToken);
            if (transaction is not null)
                await transaction.CommitAsync(cancellationToken);
            return affectedRows;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(tenant => tenant.Id);
                entity.HasIndex(tenant => tenant.Slug).IsUnique();
                entity.Property(tenant => tenant.Name).HasMaxLength(200).IsRequired();
                entity.Property(tenant => tenant.Slug).HasMaxLength(80).IsRequired();
                entity.HasData(new Tenant
                {
                    Id = 1,
                    Name = "SmartProd Demo",
                    Slug = "smartprod-demo",
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                });
            });

            modelBuilder.Entity<AuditEvent>(entity =>
            {
                entity.HasKey(audit => audit.Id);
                entity.HasIndex(audit => new { audit.TenantId, audit.OccurredAt });
                entity.HasIndex(audit => new { audit.TenantId, audit.EntityType, audit.EntityId });
                entity.Property(audit => audit.Action).HasMaxLength(40).IsRequired();
                entity.Property(audit => audit.EntityType).HasMaxLength(200).IsRequired();
                entity.Property(audit => audit.EntityId).HasMaxLength(100).IsRequired();
            });

            modelBuilder.Entity<WorkCenter>(entity =>
            {
                entity.HasKey(workCenter => workCenter.Id);
                entity.HasIndex(workCenter => new { workCenter.TenantId, workCenter.Code }).IsUnique();
                entity.Property(workCenter => workCenter.Code).HasMaxLength(80).IsRequired();
                entity.Property(workCenter => workCenter.Name).HasMaxLength(200).IsRequired();
            });

            modelBuilder.Entity<Machine>(entity =>
            {
                entity.HasKey(machine => machine.Id);
                entity.HasIndex(machine => new { machine.TenantId, machine.Code }).IsUnique();
                entity.Property(machine => machine.Code).HasMaxLength(80).IsRequired();
                entity.Property(machine => machine.Name).HasMaxLength(200).IsRequired();
                entity.Property(machine => machine.Status).HasMaxLength(40).IsRequired();
                entity.HasOne(machine => machine.WorkCenter)
                    .WithMany()
                    .HasForeignKey(machine => machine.WorkCenterId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ProductionOperation>(entity =>
            {
                entity.HasKey(operation => operation.Id);
                entity.HasIndex(operation => new { operation.TenantId, operation.ProductId, operation.Sequence }).IsUnique();
                entity.Property(operation => operation.IdealCycleTimeSeconds).HasPrecision(12, 3);
                entity.HasOne(operation => operation.Product).WithMany().HasForeignKey(operation => operation.ProductId);
                entity.HasOne(operation => operation.WorkCenter).WithMany().HasForeignKey(operation => operation.WorkCenterId);
            });

            modelBuilder.Entity<ProductionEvent>(entity =>
            {
                entity.HasKey(productionEvent => productionEvent.Id);
                entity.HasIndex(productionEvent => new { productionEvent.TenantId, productionEvent.EventId }).IsUnique();
                entity.HasIndex(productionEvent => new { productionEvent.TenantId, productionEvent.OrderId, productionEvent.OccurredAt });
                entity.HasIndex(productionEvent =>
                    new { productionEvent.TenantId, productionEvent.MachineId, productionEvent.OccurredAt });
                entity.HasIndex(productionEvent => new { productionEvent.TenantId, productionEvent.DeviceId, productionEvent.DeviceSequence })
                    .IsUnique()
                    .HasFilter("\"DeviceId\" IS NOT NULL AND \"DeviceSequence\" IS NOT NULL");
                entity.Property(productionEvent => productionEvent.Type).HasConversion<string>().HasMaxLength(40);
                entity.Property(productionEvent => productionEvent.GoodQuantity).HasPrecision(18, 3);
                entity.Property(productionEvent => productionEvent.ScrapQuantity).HasPrecision(18, 3);
                entity.Property(productionEvent => productionEvent.ReworkQuantity).HasPrecision(18, 3);
                entity.Property(productionEvent => productionEvent.ReasonCode).HasMaxLength(100);
                entity.Property(productionEvent => productionEvent.DeviceId).HasMaxLength(120);
                entity.HasOne(productionEvent => productionEvent.Order).WithMany().HasForeignKey(productionEvent => productionEvent.OrderId);
                entity.HasOne(productionEvent => productionEvent.Operation).WithMany().HasForeignKey(productionEvent => productionEvent.OperationId);
                entity.HasOne(productionEvent => productionEvent.Machine).WithMany().HasForeignKey(productionEvent => productionEvent.MachineId);
                entity.HasOne(productionEvent => productionEvent.Operator).WithMany().HasForeignKey(productionEvent => productionEvent.OperatorId);
            });

            modelBuilder.Entity<Lot>(entity =>
            {
                entity.HasKey(lot => lot.Id);
                entity.HasIndex(lot => new { lot.TenantId, lot.ProductId, lot.Number }).IsUnique();
                entity.Property(lot => lot.Number).HasMaxLength(120).IsRequired();
                entity.Property(lot => lot.SupplierLotNumber).HasMaxLength(120);
                entity.Property(lot => lot.QuantityReceived).HasPrecision(18, 3);
                entity.Property(lot => lot.QuantityRemaining).HasPrecision(18, 3);
                entity.Property(lot => lot.UnitCost).HasPrecision(18, 4);
                entity.HasOne(lot => lot.Product).WithMany().HasForeignKey(lot => lot.ProductId);
                entity.HasOne(lot => lot.ProductionOrder).WithMany().HasForeignKey(lot => lot.ProductionOrderId);
            });

            modelBuilder.Entity<LotGenealogy>(entity =>
            {
                entity.HasKey(genealogy => genealogy.Id);
                entity.HasIndex(genealogy => new
                {
                    genealogy.TenantId,
                    genealogy.InputLotId,
                    genealogy.OutputLotId,
                    genealogy.ProductionOrderId
                }).IsUnique();
                entity.Property(genealogy => genealogy.QuantityConsumed).HasPrecision(18, 3);
                entity.HasOne(genealogy => genealogy.InputLot).WithMany().HasForeignKey(genealogy => genealogy.InputLotId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(genealogy => genealogy.OutputLot).WithMany().HasForeignKey(genealogy => genealogy.OutputLotId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(genealogy => genealogy.ProductionOrder).WithMany().HasForeignKey(genealogy => genealogy.ProductionOrderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- Usuario ---
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => new { u.TenantId, u.Email }).IsUnique();
                entity.HasIndex(u => u.RefreshTokenHash).IsUnique();
            });

            // --- Produto ---
            modelBuilder.Entity<Produto>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => new { p.TenantId, p.Code }).IsUnique();
                entity.Property(p => p.Name).IsRequired();
                entity.Property(p => p.Version).IsRowVersion();

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
                entity.HasIndex(nf => new { nf.TenantId, nf.Number }).IsUnique();
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

            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                         .Where(entityType => typeof(ITenantOwned).IsAssignableFrom(entityType.ClrType)))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "entity");
                var tenantProperty = System.Linq.Expressions.Expression.Property(parameter, nameof(ITenantOwned.TenantId));
                var currentTenant = System.Linq.Expressions.Expression.Property(
                    System.Linq.Expressions.Expression.Constant(this),
                    nameof(CurrentTenantId));
                var filter = System.Linq.Expressions.Expression.Lambda(
                    System.Linq.Expressions.Expression.Equal(tenantProperty, currentTenant),
                    parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(ITenantOwned.TenantId))
                    .HasDefaultValue(1);
                modelBuilder.Entity(entityType.ClrType)
                    .HasOne(typeof(Tenant))
                    .WithMany()
                    .HasForeignKey(nameof(ITenantOwned.TenantId))
                    .OnDelete(DeleteBehavior.Restrict);
            }
        }

        private void AssignTenantToNewEntities()
        {
            foreach (var entry in ChangeTracker.Entries<ITenantOwned>()
                         .Where(entry => entry.State == EntityState.Added && entry.Entity.TenantId == 0))
                entry.Entity.TenantId = CurrentTenantId;
        }

        private List<PendingAudit> CaptureAuditableChanges() =>
            ChangeTracker.Entries()
                .Where(entry => entry.Entity is ITenantOwned and not AuditEvent &&
                                entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .Select(entry =>
                {
                    var before = entry.State is EntityState.Modified or EntityState.Deleted
                        ? GetChangedValues(entry, useOriginalValues: true)
                        : null;
                    var after = entry.State is EntityState.Added or EntityState.Modified
                        ? GetChangedValues(entry, useOriginalValues: false)
                        : null;
                    return new PendingAudit(entry, entry.State.ToString(), before, after);
                })
                .ToList();

        private static string? GetChangedValues(EntityEntry entry, bool useOriginalValues)
        {
            var values = new Dictionary<string, object?>();
            foreach (var property in entry.Properties)
            {
                if (IsSensitiveProperty(property.Metadata.Name) ||
                    (entry.State == EntityState.Modified && !property.IsModified))
                    continue;

                values[property.Metadata.Name] = useOriginalValues
                    ? property.OriginalValue
                    : property.CurrentValue;
            }

            return values.Count == 0 ? null : JsonSerializer.Serialize(values);
        }

        private static bool IsSensitiveProperty(string name) =>
            name.Contains("password", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("senha", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("token", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("secret", StringComparison.OrdinalIgnoreCase);

        private void PersistAuditEvents(List<PendingAudit> pendingAudit, bool acceptAllChangesOnSuccess)
        {
            if (pendingAudit.Count == 0)
            {
                if (acceptAllChangesOnSuccess)
                    ChangeTracker.AcceptAllChanges();
                return;
            }

            var trackedStates = CaptureTrackedStates(pendingAudit);
            foreach (var trackedState in trackedStates)
            {
                var entry = trackedState.Entry;
                entry.State = EntityState.Unchanged;
            }

            AuditEvents.AddRange(pendingAudit.Select(CreateAuditEvent));
            base.SaveChanges(acceptAllChangesOnSuccess: true);
            RestoreTrackedStates(trackedStates, acceptAllChangesOnSuccess);
        }

        private async Task PersistAuditEventsAsync(
            List<PendingAudit> pendingAudit,
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken)
        {
            if (pendingAudit.Count == 0)
            {
                if (acceptAllChangesOnSuccess)
                    ChangeTracker.AcceptAllChanges();
                return;
            }

            var trackedStates = CaptureTrackedStates(pendingAudit);
            foreach (var trackedState in trackedStates)
            {
                var entry = trackedState.Entry;
                entry.State = EntityState.Unchanged;
            }

            AuditEvents.AddRange(pendingAudit.Select(CreateAuditEvent));
            await base.SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken);
            RestoreTrackedStates(trackedStates, acceptAllChangesOnSuccess);
        }

        private static List<TrackedEntityState> CaptureTrackedStates(List<PendingAudit> pendingAudit) =>
            pendingAudit.Select(audit => audit.Entry).Distinct()
                .Select(entry => new TrackedEntityState(
                    entry,
                    entry.State,
                    entry.CurrentValues.Clone(),
                    entry.OriginalValues.Clone(),
                    entry.Properties.ToDictionary(property => property.Metadata.Name, property => property.IsModified)))
                .ToList();

        private static void RestoreTrackedStates(
            List<TrackedEntityState> trackedStates,
            bool acceptAllChangesOnSuccess)
        {
            foreach (var trackedState in trackedStates)
            {
                var entry = trackedState.Entry;
                entry.State = EntityState.Unchanged;
                entry.CurrentValues.SetValues(trackedState.CurrentValues);

                if (acceptAllChangesOnSuccess)
                {
                    entry.OriginalValues.SetValues(trackedState.CurrentValues);
                    if (trackedState.State == EntityState.Deleted)
                        entry.State = EntityState.Detached;
                    else
                        entry.State = EntityState.Unchanged;
                    continue;
                }

                entry.OriginalValues.SetValues(trackedState.OriginalValues);
                entry.State = trackedState.State;
                if (trackedState.State == EntityState.Modified)
                {
                    foreach (var (propertyName, isModified) in trackedState.ModifiedProperties)
                        entry.Property(propertyName).IsModified = isModified;
                }
            }
        }

        private AuditEvent CreateAuditEvent(PendingAudit pending)
        {
            var key = pending.Entry.Metadata.FindPrimaryKey()?.Properties
                .Select(property => pending.Entry.Property(property.Name).CurrentValue)
                .FirstOrDefault()?.ToString() ?? "unknown";
            var user = _httpContextAccessor?.HttpContext?.User;
            var actorId = int.TryParse(user?.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedId)
                ? parsedId
                : (int?)null;

            return new AuditEvent
            {
                TenantId = ((ITenantOwned)pending.Entry.Entity).TenantId,
                ActorUserId = actorId,
                ActorName = user?.Identity?.Name ?? user?.FindFirstValue(ClaimTypes.Email) ?? "system",
                Action = pending.Action,
                EntityType = pending.Entry.Metadata.ClrType.Name,
                EntityId = key,
                BeforeJson = pending.BeforeJson,
                AfterJson = pending.AfterJson,
                CorrelationId = _httpContextAccessor?.HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N"),
                OccurredAt = DateTime.UtcNow
            };
        }

        private sealed record PendingAudit(EntityEntry Entry, string Action, string? BeforeJson, string? AfterJson);
        private sealed record TrackedEntityState(
            EntityEntry Entry,
            EntityState State,
            PropertyValues CurrentValues,
            PropertyValues OriginalValues,
            Dictionary<string, bool> ModifiedProperties);
    }
}
