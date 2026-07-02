using Adopta.Domain.Authoring;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adopta.Infrastructure.Persistence.Configurations;

public sealed class AuthoredContentItemConfiguration : IEntityTypeConfiguration<AuthoredContentItem>
{
    public void Configure(EntityTypeBuilder<AuthoredContentItem> builder)
    {
        builder.ToTable("AuthoredContentItems");
        builder.HasKey(content => content.Id);
        builder.Property(content => content.Id).ValueGeneratedNever();
        builder.Property(content => content.TenantId).IsRequired();
        builder.Property(content => content.ApplicationId).IsRequired();
        builder.Property(content => content.ContentType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(content => content.ContentKey).HasMaxLength(200).IsRequired();
        builder.Property(content => content.Title).HasMaxLength(300).IsRequired();
        builder.HasIndex(content => new { content.TenantId, content.Id }).IsUnique();
        builder.HasIndex(content => new { content.TenantId, content.ApplicationId, content.ContentKey }).IsUnique();

        builder.OwnsMany(content => content.Versions, AuthoredContentVersionConfiguration.Configure);
        builder.Navigation(content => content.Versions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
