using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adopta.Infrastructure.Persistence.Configurations;

public sealed class AuthoredContentPublishingHistoryRecordConfiguration
    : IEntityTypeConfiguration<AuthoredContentPublishingHistoryRecord>
{
    public void Configure(EntityTypeBuilder<AuthoredContentPublishingHistoryRecord> builder)
    {
        builder.ToTable("AuthoredContentPublishingHistory");
        builder.HasKey(record => record.Id);
        builder.Property(record => record.Id).ValueGeneratedNever();
        builder.Property(record => record.TenantId).IsRequired();
        builder.Property(record => record.ContentId).IsRequired();
        builder.Property(record => record.VersionId).IsRequired();
        builder.Property(record => record.ActorUserId).IsRequired();
        builder.Property(record => record.Environment).HasMaxLength(80).IsRequired();
        builder.Property(record => record.Channel).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(record => record.Result).HasMaxLength(80).IsRequired();
        builder.Property(record => record.OccurredAtUtc).IsRequired();
        builder.HasIndex(record => new { record.TenantId, record.OccurredAtUtc });
        builder.HasIndex(record => new { record.TenantId, record.ContentId, record.VersionId });
        builder.HasIndex(record => new { record.TenantId, record.Environment, record.Channel });
    }
}
