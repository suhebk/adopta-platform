using Adopta.Application.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adopta.Infrastructure.Persistence.Configurations;

public sealed class SecurityAuditEventConfiguration : IEntityTypeConfiguration<SecurityAuditEventRecord>
{
    public void Configure(EntityTypeBuilder<SecurityAuditEventRecord> builder)
    {
        builder.ToTable("SecurityAuditEvents");
        builder.HasKey(auditEvent => new
        {
            auditEvent.TenantId,
            auditEvent.OccurredAtUtc,
            auditEvent.Action,
            auditEvent.Outcome
        });
        builder.Property(auditEvent => auditEvent.TenantId).IsRequired();
        builder.Property(auditEvent => auditEvent.OccurredAtUtc).IsRequired();
        builder.Property(auditEvent => auditEvent.Action).HasMaxLength(160).IsRequired();
        builder.Property(auditEvent => auditEvent.Outcome).HasMaxLength(80).IsRequired();
        builder.Property(auditEvent => auditEvent.FailureCategory).HasMaxLength(120);
        builder.HasIndex(auditEvent => new { auditEvent.TenantId, auditEvent.OccurredAtUtc });
    }
}
