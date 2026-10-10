using Base.Api.Infrastructure;
using Base.BuildingBlocks;
using Base.Modules.Todos;

var builder = WebApplication.CreateBuilder(args);

// Every business module is listed here. To add one, create a project under src/Modules,
// implement IModule, reference it from this project and add it to this array.
IModule[] modules =
[
    new TodosModule(),
];

builder.AddPlatformDefaults();

foreach (var module in modules)
{
    module.AddServices(builder.Services, builder.Configuration);
}

var app = builder.Build();

app.UsePlatformDefaults();

var api = app.MapGroup("/api");
foreach (var module in modules)
{
    module.MapEndpoints(api);
}

foreach (var module in modules)
{
    await module.InitializeAsync(app.Services, app.Lifetime.ApplicationStopping);
}

await app.RunAsync();

// Lets the integration tests start the app with WebApplicationFactory<Program>.
public partial class Program;
