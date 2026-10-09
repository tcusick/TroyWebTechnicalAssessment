using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RentalApplications.Data;

namespace RentalApplications.Health;

/// <summary>
/// Registers and maps health check endpoints for load balancers and orchestrators.
/// </summary>
public static class HealthCheckConfiguration
{
    /// <summary>
    /// Adds a database connectivity check via <see cref="AppDbContext"/>.
    /// </summary>
    public static IServiceCollection AddRentalApplicationsHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>(
                name: "database",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready"]);

        return services;
    }

    /// <summary>
    /// Maps <c>/health</c> (all checks) and <c>/health/ready</c> (database only). Anonymous access.
    /// </summary>
    public static WebApplication MapRentalApplicationsHealthChecks(this WebApplication app)
    {
        static Task WriteMinimalJson(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json";
            var status = report.Status == HealthStatus.Healthy ? "Healthy" : "Unhealthy";
            return context.Response.WriteAsJsonAsync(new { status, totalDuration = report.TotalDuration.TotalMilliseconds });
        }

        var options = new HealthCheckOptions { ResponseWriter = WriteMinimalJson };

        app.MapHealthChecks("/health", options);
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("ready"),
            ResponseWriter = WriteMinimalJson
        });

        return app;
    }
}
