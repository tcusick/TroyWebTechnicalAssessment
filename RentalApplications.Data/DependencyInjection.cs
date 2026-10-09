using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RentalApplications.Data;

/// <summary>
/// Registers EF Core data access with dependency injection.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Configures <see cref="AppDbContext"/> with SQL Server using the given connection string.
    /// </summary>
    public static IServiceCollection AddRentalApplicationsData(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));
        return services;
    }
}
