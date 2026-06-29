namespace Adopta.Infrastructure.Persistence;

public sealed class AdoptaPersistenceOptions
{
    public const string SectionName = "Persistence";

    public bool Enabled { get; set; }

    public string? Provider { get; set; }

    public SqlServerPersistenceOptions SqlServer { get; set; } = new();
}

public sealed class SqlServerPersistenceOptions
{
    public string? ConnectionStringName { get; set; }
}
