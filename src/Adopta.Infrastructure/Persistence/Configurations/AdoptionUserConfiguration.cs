using Adopta.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adopta.Infrastructure.Persistence.Configurations;

public sealed class AdoptionUserConfiguration : IEntityTypeConfiguration<AdoptionUser>
{
    public void Configure(EntityTypeBuilder<AdoptionUser> builder)
    {
        builder.ToTable("AdoptionUsers");
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).ValueGeneratedNever();
        builder.Property(user => user.TenantId).IsRequired();
        builder.Property(user => user.ExternalUserId).HasMaxLength(256).IsRequired();
        builder.Property(user => user.DisplayName).HasMaxLength(200).IsRequired();
        builder.HasIndex(user => new { user.TenantId, user.Id }).IsUnique();
        builder.HasIndex(user => new { user.TenantId, user.ExternalUserId }).IsUnique();

        builder.HasMany(user => user.Roles)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "AdoptionUserRoles",
                right => right.HasOne<Role>().WithMany().HasForeignKey("RoleId").OnDelete(DeleteBehavior.Restrict),
                left => left.HasOne<AdoptionUser>().WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("AdoptionUserRoles");
                    join.HasKey("UserId", "RoleId");
                    join.IndexerProperty<Guid>("TenantId").IsRequired();
                    join.HasIndex("TenantId", "UserId", "RoleId").IsUnique();
                });

        builder.Navigation(user => user.Roles).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
