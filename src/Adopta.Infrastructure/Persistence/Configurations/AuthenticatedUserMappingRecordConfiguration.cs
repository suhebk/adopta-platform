using Adopta.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adopta.Infrastructure.Persistence.Configurations;

public sealed class AuthenticatedUserMappingRecordConfiguration : IEntityTypeConfiguration<AuthenticatedUserMappingEntity>
{
    public void Configure(EntityTypeBuilder<AuthenticatedUserMappingEntity> builder)
    {
        builder.ToTable("AuthenticatedUserMappings");
        builder.HasKey(mapping => new { mapping.TenantId, mapping.ExternalSubjectId });
        builder.Property(mapping => mapping.TenantId).IsRequired();
        builder.Property(mapping => mapping.ExternalSubjectId).HasMaxLength(256).IsRequired();
        builder.Property(mapping => mapping.UserId).IsRequired();
        builder.HasOne<AdoptionUser>()
            .WithMany()
            .HasForeignKey(mapping => mapping.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(mapping => new { mapping.TenantId, mapping.ExternalSubjectId }).IsUnique();
        builder.HasIndex(mapping => new { mapping.TenantId, mapping.UserId }).IsUnique();
    }
}
