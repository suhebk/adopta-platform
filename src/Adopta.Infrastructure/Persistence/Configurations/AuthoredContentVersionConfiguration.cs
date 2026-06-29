using Adopta.Domain.Authoring;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adopta.Infrastructure.Persistence.Configurations;

public static class AuthoredContentVersionConfiguration
{
    public static void Configure(OwnedNavigationBuilder<AuthoredContentItem, AuthoredContentVersion> builder)
    {
        builder.ToTable("AuthoredContentVersions");
        builder.WithOwner().HasForeignKey("ContentId");
        builder.Property<Guid>("ContentId").IsRequired();
        builder.Property<Guid>("TenantId").IsRequired();
        builder.Property(version => version.Id).ValueGeneratedNever();
        builder.Property(version => version.Version).HasMaxLength(64).IsRequired();
        builder.Property(version => version.LifecycleState).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(version => version.CreatedAtUtc).IsRequired();
        builder.HasKey("ContentId", nameof(AuthoredContentVersion.Id));
        builder.HasIndex("TenantId", "ContentId");
        builder.HasIndex("ContentId", nameof(AuthoredContentVersion.Version)).IsUnique();
    }
}
