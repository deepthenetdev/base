using Microsoft.Extensions.Configuration;

namespace Base.BuildingBlocks;

public static class ConfigurationExtensions
{
    /// <summary>
    /// Reads a connection string and fails fast with a clear message when it is missing,
    /// instead of failing later with a confusing driver error.
    /// </summary>
    public static string GetRequiredConnectionString(this IConfiguration configuration, string name)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var value = configuration.GetConnectionString(name);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Connection string '{name}' is missing. Set 'ConnectionStrings:{name}' in appsettings " +
                $"or the environment variable 'ConnectionStrings__{name}'.");
        }

        return value;
    }
}
