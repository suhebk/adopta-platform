using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adopta.Infrastructure.Persistence.Configurations;

public sealed class AuthoredContentLifecycleHistoryRecordConfiguration
    : IEntityTypeConfiguration<AuthoredContentLifecycleHistoryRecord>
{
    public void Configure(EntityTypeBuilder<AuthoredContentLifecycleHistoryRecord> builder)
    {
        builder.ToTable("AuthoredContentLifecycleHistory");
        builder.HasKey(record => record.Id);
        builder.Property(record => record.Id).ValueGeneratedNever();
        builder.Property(record => record.TenantId).IsRequired();
        builder.Property(record => record.ContentId).IsRequired();
        builder.Property(record => record.VersionId);
        builder.Property(record => record.ActorUserId).IsRequired();
        builder.Property(record => record.LifecycleAction).HasMaxLength(80).IsRequired();
        builder.Property(record => record.FromState).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(record => record.ToState).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(record => record.Result).HasMaxLength(80).IsRequired();
        builder.Property(record => record.OccurredAtUtc).IsRequired();
        builder.HasIndex(record => new { record.TenantId, record.OccurredAtUtc });
        builder.HasIndex(record => new { record.TenantId, record.ContentId, record.VersionId });
    }
}
