using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Base.Api.Tests;

/// <summary>
/// Starts the real API in memory for integration tests. It talks to the PostgreSQL and Valkey from
/// docker compose (locally) or the service containers in CI, using the Development connection strings.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("Database:ApplyMigrationsOnStartup", "true");
    }
}
