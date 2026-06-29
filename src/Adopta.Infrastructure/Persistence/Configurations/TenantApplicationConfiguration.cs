using Adopta.Domain.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Adopta.Infrastructure.Persistence.Configurations;

public sealed class TenantApplicationConfiguration : IEntityTypeConfiguration<TenantApplication>
{
    public void Configure(EntityTypeBuilder<TenantApplication> builder)
    {
        builder.ToTable("TenantApplications");
        builder.HasKey(application => application.Id);
        builder.Property(application => application.Id).ValueGeneratedNever();
        builder.Property(application => application.TenantId).IsRequired();
        builder.Property(application => application.Name).HasMaxLength(200).IsRequired();
        builder.Property(application => application.AllowedOrigin)
            .HasConversion(origin => origin.ToString(), value => new Uri(value))
            .HasMaxLength(2048)
            .IsRequired();
        builder.Property(application => application.CreatedAtUtc).IsRequired();
        builder.HasIndex(application => new { application.TenantId, application.Id }).IsUnique();
        builder.HasIndex(application => new { application.TenantId, application.Name });
    }
}
