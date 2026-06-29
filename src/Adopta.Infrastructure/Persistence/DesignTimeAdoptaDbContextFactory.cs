using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Adopta.Infrastructure.Persistence;

public sealed class DesignTimeAdoptaDbContextFactory : IDesignTimeDbContextFactory<AdoptaDbContext>
{
    public AdoptaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AdoptaDbContext>();

        // Placeholder only for EF design-time model generation. This value is not
        // a real connection string and does not affect runtime DI.
        optionsBuilder.UseSqlServer("__adopta_design_time_placeholder__");

        return new AdoptaDbContext(optionsBuilder.Options);
    }
}
