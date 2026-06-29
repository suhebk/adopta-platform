using Adopta.Domain.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adopta.Infrastructure.Persistence.Configurations;

public sealed class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("AuditEvents");
        builder.HasKey(auditEvent => auditEvent.Id);
        builder.Property(auditEvent => auditEvent.Id).ValueGeneratedNever();
        builder.Property(auditEvent => auditEvent.TenantId).IsRequired();
        builder.Property(auditEvent => auditEvent.ActorUserId).IsRequired();
        builder.Property(auditEvent => auditEvent.Action).HasMaxLength(160).IsRequired();
        builder.Property(auditEvent => auditEvent.TargetType).HasMaxLength(160).IsRequired();
        builder.Property(auditEvent => auditEvent.TargetId).HasMaxLength(256).IsRequired();
        builder.Property(auditEvent => auditEvent.OccurredAtUtc).IsRequired();
        builder.HasIndex(auditEvent => new { auditEvent.TenantId, auditEvent.Id }).IsUnique();
        builder.HasIndex(auditEvent => new { auditEvent.TenantId, auditEvent.OccurredAtUtc });
    }
}
