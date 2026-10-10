using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Base.BuildingBlocks;

/// <summary>
/// A business module in the modular monolith. Each module owns its endpoints, its data (its own
/// database schema) and its services. Modules never reference each other's internals, so any module
/// can later be extracted into its own service (talking over gRPC or Kafka) without rewriting the rest.
/// </summary>
public interface IModule
{
    /// <summary>Short, unique name, used for logging and as the database schema name.</summary>
    string Name { get; }

    /// <summary>Registers the module's services (DbContext, handlers, options).</summary>
    void AddServices(IServiceCollection services, IConfiguration configuration);

    /// <summary>Maps the module's HTTP endpoints under the shared <c>/api</c> group.</summary>
    void MapEndpoints(IEndpointRouteBuilder endpoints);

    /// <summary>Runs once at startup, after the app is built (for example, applying database migrations).</summary>
    Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken) => Task.CompletedTask;
}
