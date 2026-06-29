using Adopta.Application.Audit;
using Adopta.Application.Identity;
using Adopta.Domain.Audit;
using Adopta.Domain.Authoring;
using Adopta.Domain.Identity;
using Adopta.Domain.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace Adopta.Infrastructure.Persistence;

public sealed class AdoptaDbContext : DbContext
{
    public AdoptaDbContext(DbContextOptions<AdoptaDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<TenantApplication> TenantApplications => Set<TenantApplication>();

    public DbSet<AdoptionUser> AdoptionUsers => Set<AdoptionUser>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<TenantMappingRecord> TenantMappings => Set<TenantMappingRecord>();

    public DbSet<AuthenticatedUserMappingEntity> AuthenticatedUserMappings => Set<AuthenticatedUserMappingEntity>();

    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    public DbSet<SecurityAuditEventRecord> SecurityAuditEvents => Set<SecurityAuditEventRecord>();

    public DbSet<AuthoredContentItem> AuthoredContentItems => Set<AuthoredContentItem>();

    public DbSet<AuthoredContentLifecycleHistoryRecord> AuthoredContentLifecycleHistory =>
        Set<AuthoredContentLifecycleHistoryRecord>();

    public DbSet<AuthoredContentPublishingHistoryRecord> AuthoredContentPublishingHistory =>
        Set<AuthoredContentPublishingHistoryRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("adopta");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AdoptaDbContext).Assembly);
    }
}
