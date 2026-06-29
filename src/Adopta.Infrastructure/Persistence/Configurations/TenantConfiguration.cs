using Adopta.Domain.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adopta.Infrastructure.Persistence.Configurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(tenant => tenant.Id);
        builder.Property(tenant => tenant.Id).ValueGeneratedNever();
        builder.Property(tenant => tenant.Name).HasMaxLength(200).IsRequired();
        builder.Property(tenant => tenant.PrimaryDomain).HasMaxLength(255).IsRequired();
        builder.Property(tenant => tenant.DataRegion).HasMaxLength(64).IsRequired();
        builder.Property(tenant => tenant.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(tenant => tenant.CreatedAtUtc).IsRequired();
        builder.HasIndex(tenant => tenant.PrimaryDomain).IsUnique();
    }
}
