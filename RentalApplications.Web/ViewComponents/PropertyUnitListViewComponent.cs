using Microsoft.AspNetCore.Mvc;
using RentalApplications.Services.Abstractions;

namespace RentalApplications.ViewComponents;

/// <summary>
/// Renders the unit list for a property on the manager properties page.
/// </summary>
public class PropertyUnitListViewComponent : ViewComponent
{
    private readonly IPropertyManagementService _properties;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyUnitListViewComponent"/> class.
    /// </summary>
    public PropertyUnitListViewComponent(IPropertyManagementService properties)
    {
        _properties = properties;
    }

    /// <summary>
    /// Loads units for the property and returns the default view.
    /// </summary>
    /// <param name="propertyId">Property identifier.</param>
    public async Task<IViewComponentResult> InvokeAsync(int propertyId)
    {
        var units = await _properties.GetUnitsForPropertyAsync(propertyId);
        return View(units);
    }
}
