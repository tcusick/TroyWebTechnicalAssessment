using Microsoft.Extensions.DependencyInjection;
using RentalApplications.Services.Abstractions;

namespace RentalApplications.Services;

/// <summary>
/// Registers application services with dependency injection.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds rental application service implementations to the service collection.
    /// </summary>
    public static IServiceCollection AddRentalApplicationsServices(this IServiceCollection services)
    {
        services.AddScoped<ILeaseAvailabilityService, LeaseAvailabilityService>();
        services.AddScoped<IPropertyManagementService, PropertyManagementService>();
        services.AddScoped<IRentalApplicationService, RentalApplicationService>();
        services.AddScoped<IAvailableUnitsService, AvailableUnitsService>();
        services.AddScoped<IResidenceHistoryService, ResidenceHistoryService>();
        services.AddScoped<IApplicationStatusHistoryService, ApplicationStatusHistoryService>();
        return services;
    }
}
