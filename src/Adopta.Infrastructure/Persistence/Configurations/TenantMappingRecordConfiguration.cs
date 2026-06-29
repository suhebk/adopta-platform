using Adopta.Application.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adopta.Infrastructure.Persistence.Configurations;

public sealed class TenantMappingRecordConfiguration : IEntityTypeConfiguration<TenantMappingRecord>
{
    public void Configure(EntityTypeBuilder<TenantMappingRecord> builder)
    {
        builder.ToTable("TenantMappings");
        builder.HasKey(mapping => new { mapping.TenantId, mapping.ExternalTenantId, mapping.ApplicationId });
        builder.Property(mapping => mapping.TenantId).IsRequired();
        builder.Property(mapping => mapping.ExternalTenantId).HasMaxLength(256).IsRequired();
        builder.Property(mapping => mapping.ApplicationId).HasMaxLength(256).IsRequired();
        builder.HasIndex(mapping => new { mapping.TenantId, mapping.ExternalTenantId, mapping.ApplicationId }).IsUnique();
    }
}
