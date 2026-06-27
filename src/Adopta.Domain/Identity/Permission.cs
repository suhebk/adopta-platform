namespace Adopta.Domain.Identity;

public sealed class Permission
{
    public Permission(string key, string description)
    {
        Key = string.IsNullOrWhiteSpace(key)
            ? throw new ArgumentException("Permission key is required.", nameof(key))
            : key.Trim();
        Description = string.IsNullOrWhiteSpace(description)
            ? throw new ArgumentException("Permission description is required.", nameof(description))
            : description.Trim();
    }

    public string Key { get; }

    public string Description { get; }
}
