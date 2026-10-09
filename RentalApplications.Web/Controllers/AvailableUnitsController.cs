using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalApplications.Domain;
using RentalApplications.Mapping;
using RentalApplications.Models.AvailableUnits;
using RentalApplications.Services.Abstractions;

namespace RentalApplications.Controllers;

/// <summary>
/// Browse rentable units that are not blocked by an active lease on the current date.
/// </summary>
[Authorize(Roles = AppRoles.Applicant)]
public class AvailableUnitsController : Controller
{
    private readonly IAvailableUnitsService _availableUnits;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvailableUnitsController"/> class.
    /// </summary>
    public AvailableUnitsController(IAvailableUnitsService availableUnits)
    {
        _availableUnits = availableUnits;
    }

    /// <summary>
    /// Lists units available for new rental applications.
    /// </summary>
    /// <returns>The available units view.</returns>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var units = await _availableUnits.GetAvailableUnitsAsync();
        var model = units.Select(u => u.ToViewModel()).ToList();

        return View(model);
    }
}
