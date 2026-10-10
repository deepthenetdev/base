using System.Threading.RateLimiting;
using Base.BuildingBlocks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Caching.Hybrid;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;

namespace Base.Api.Infrastructure;

/// <summary>
/// Cross-cutting setup every app built on this base gets for free: consistent error responses,
/// OpenAPI docs, caching, health checks, rate limiting and telemetry.
/// </summary>
public static class PlatformDefaults
{
    public const string ReadyTag = "ready";

    public static WebApplicationBuilder AddPlatformDefaults(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var services = builder.Services;
        var configuration = builder.Configuration;

        // Errors: every failure becomes an RFC 9457 "problem details" JSON response.
        services.AddProblemDetails();

        // Lets code take the current time as a dependency, so tests can control it.
        services.AddSingleton(TimeProvider.System);

        // OpenAPI document at /openapi/v1.json; the frontend's typed client can be generated from it.
        services.AddOpenApi();

        // Caching: HybridCache keeps a small in-memory copy and uses Valkey as the shared second level,
        // so every API replica sees the same cache.
        var valkey = configuration.GetRequiredConnectionString("Valkey");
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = valkey;
            options.InstanceName = "base:";
        });
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromSeconds(30),
            };
        });

        // Health: /health/live says the process is up; /health/ready also checks its dependencies.
        services.AddHealthChecks()
            .AddNpgSql(configuration.GetRequiredConnectionString("Postgres"), name: "postgres", tags: [ReadyTag])
            .AddRedis(valkey, name: "valkey", tags: [ReadyTag]);

        // Rate limiting: a per-client fixed window protects the API from accidental floods.
        var permitsPerMinute = configuration.GetValue("RateLimiting:PermitsPerMinute", 600);
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitsPerMinute,
                        Window = TimeSpan.FromMinutes(1),
                    }));
        });

        builder.AddPlatformTelemetry();

        return builder;
    }

    public static WebApplication UsePlatformDefaults(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseExceptionHandler();
        app.UseStatusCodePages();
        app.UseRateLimiter();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(); // Interactive API docs at /scalar
        }

        app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains(ReadyTag),
        });

        return app;
    }

    private static void AddPlatformTelemetry(this WebApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        var telemetry = builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation())
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource("Npgsql"));

        // Export only when a collector is configured (for example the Aspire dashboard in docker compose).
        if (!string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
        {
            telemetry.UseOtlpExporter();
        }
    }
}
