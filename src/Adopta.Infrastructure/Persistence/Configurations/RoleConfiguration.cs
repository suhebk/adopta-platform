using Adopta.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adopta.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(role => role.Id);
        builder.Property(role => role.Id).ValueGeneratedNever();
        builder.Property(role => role.TenantId).IsRequired();
        builder.Property(role => role.Name).HasMaxLength(160).IsRequired();
        builder.HasIndex(role => new { role.TenantId, role.Id }).IsUnique();
        builder.HasIndex(role => new { role.TenantId, role.Name }).IsUnique();

        builder.HasMany(role => role.Permissions)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "RolePermissions",
                right => right.HasOne<Permission>().WithMany().HasForeignKey("PermissionKey").OnDelete(DeleteBehavior.Restrict),
                left => left.HasOne<Role>().WithMany().HasForeignKey("RoleId").OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("RolePermissions");
                    join.HasKey("RoleId", "PermissionKey");
                    join.IndexerProperty<Guid>("TenantId").IsRequired();
                    join.IndexerProperty<string>("PermissionKey").HasMaxLength(160).IsRequired();
                    join.HasIndex("TenantId", "RoleId", "PermissionKey").IsUnique();
                });

        builder.Navigation(role => role.Permissions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
