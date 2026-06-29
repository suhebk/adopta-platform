using Adopta.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adopta.Infrastructure.Persistence.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");
        builder.HasKey(permission => permission.Key);
        builder.Property(permission => permission.Key).HasMaxLength(160).IsRequired();
        builder.Property(permission => permission.Description).HasMaxLength(512).IsRequired();
    }
}
