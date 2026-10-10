using Base.BuildingBlocks;
using Base.Modules.Todos.Data;
using Base.Modules.Todos.Endpoints;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Base.Modules.Todos;

public sealed class TodosModule : IModule
{
    public string Name => TodosDbContext.Schema;

    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetRequiredConnectionString("Postgres");

        services.AddDbContext<TodosDbContext>(options => options.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", TodosDbContext.Schema)));
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) => endpoints.MapTodoEndpoints();

    public async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        if (!configuration.GetValue("Database:ApplyMigrationsOnStartup", false))
        {
            return;
        }

        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TodosDbContext>();

        // Once you add EF Core migrations (see README), they are applied here.
        // Until then, the schema is created directly from the model.
        if (db.Database.GetMigrations().Any())
        {
            await db.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await db.Database.EnsureCreatedAsync(cancellationToken);
        }
    }
}
