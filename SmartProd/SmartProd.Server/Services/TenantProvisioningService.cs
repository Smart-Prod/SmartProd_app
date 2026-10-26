using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SmartProd.API.Server.Data;
using SmartProd.API.Server.DTOs;
using SmartProd.API.Server.Enum;
using SmartProd.API.Server.Models;
using SmartProd.API.Server.Utils;

namespace SmartProd.API.Server.Services;

public sealed class TenantProvisioningService(AppDbContext context)
{
    public async Task<Tenant> CreateTenantAsync(
        TenantProvisionDto dto,
        CancellationToken cancellationToken)
    {
        var name = dto.Name.Trim();
        var slug = dto.Slug.Trim().ToLowerInvariant();
        var email = dto.AdminEmail.Trim();
        if (!new EmailAddressAttribute().IsValid(email))
            throw new ValidationException("The tenant administrator email is invalid.");
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(slug) ||
            string.IsNullOrWhiteSpace(dto.AdminName) || string.IsNullOrWhiteSpace(dto.AdminPassword))
            throw new ValidationException("Tenant and administrator fields are required.");
        if (dto.AdminPassword.Length < 12)
            throw new ValidationException("The tenant administrator password must contain at least 12 characters.");
        if (await context.Tenants.AnyAsync(tenant => tenant.Slug == slug, cancellationToken))
            throw new ConflictException("A tenant with this slug already exists.");
        if (await context.Usuarios.IgnoreQueryFilters()
                .AnyAsync(user => user.Email == email, cancellationToken))
            throw new ConflictException("A user with this email already exists.");

        await using var transaction = context.Database.IsRelational()
            ? await context.Database.BeginTransactionAsync(cancellationToken)
            : null;
        var tenant = new Tenant
        {
            Name = name,
            Slug = slug,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync(cancellationToken);

        var admin = new Usuario
        {
            TenantId = tenant.Id,
            Name = dto.AdminName.Trim(),
            Email = email,
            Senha = BCrypt.Net.BCrypt.HashPassword(dto.AdminPassword),
            Active = true,
            CreatedAt = DateTime.UtcNow,
            Role = UserRole.Admin
        };
        context.Usuarios.Add(admin);
        await context.SaveChangesAsync(cancellationToken);

        context.AuditEvents.Add(new AuditEvent
        {
            TenantId = tenant.Id,
            ActorUserId = null,
            ActorName = "platform",
            Action = "Provisioned",
            EntityType = nameof(Tenant),
            EntityId = tenant.Id.ToString(),
            AfterJson = System.Text.Json.JsonSerializer.Serialize(new { tenant.Name, tenant.Slug }),
            CorrelationId = Guid.NewGuid().ToString("N"),
            OccurredAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync(cancellationToken);
        if (transaction is not null)
            await transaction.CommitAsync(cancellationToken);

        return tenant;
    }
}
