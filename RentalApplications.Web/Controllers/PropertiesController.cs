using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalApplications.Domain;
using RentalApplications.Domain.Entities;
using RentalApplications.Mapping;
using RentalApplications.Models.Properties;
using RentalApplications.Services.Abstractions;

namespace RentalApplications.Controllers;

/// <summary>
/// Property and unit maintenance for property managers (including modal CRUD).
/// </summary>
[Authorize(Roles = AppRoles.PropertyManager)]
public class PropertiesController : Controller
{
    private readonly IPropertyManagementService _properties;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertiesController"/> class.
    /// </summary>
    public PropertiesController(IPropertyManagementService properties)
    {
        _properties = properties;
    }

    /// <summary>
    /// Lists all properties and their units.
    /// </summary>
    /// <returns>The properties index view.</returns>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var properties = await _properties.GetPropertiesWithUnitsAsync();
        return View(properties);
    }

    /// <summary>
    /// Returns an empty property form partial for create.
    /// </summary>
    /// <returns>The property form partial view.</returns>
    [HttpGet]
    public IActionResult CreatePropertyModal() =>
        PartialView("_PropertyForm", new PropertyFormViewModel());

    /// <summary>
    /// Returns a property form partial populated for edit.
    /// </summary>
    /// <param name="id">Property identifier.</param>
    /// <returns>The property form partial, or <see cref="NotFoundResult"/>.</returns>
    [HttpGet]
    public async Task<IActionResult> EditPropertyModal(int id)
    {
        var property = await _properties.GetPropertyAsync(id);
        return property is null ? NotFound() : PartialView("_PropertyForm", property.ToFormViewModel());
    }

    /// <summary>
    /// Creates or updates a property.
    /// </summary>
    /// <param name="model">Property fields from the modal form.</param>
    /// <returns>JSON success, the partial on validation failure, or <see cref="NotFoundResult"/>.</returns>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveProperty(PropertyFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return PartialView("_PropertyForm", model);
        }

        var property = model.Id is int id and > 0
            ? await _properties.GetPropertyAsync(id) ?? new Property()
            : new Property();

        if (model.Id is int existingId and > 0 && property.Id == 0)
        {
            return NotFound();
        }

        property.ApplyForm(model);

        await _properties.SavePropertyAsync(property);
        return Json(new { success = true });
    }

    /// <summary>
    /// Deletes a property when it has no units.
    /// </summary>
    /// <param name="id">Property identifier.</param>
    /// <returns>JSON success or a bad request with an error message.</returns>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProperty(int id)
    {
        var error = await _properties.TryDeletePropertyAsync(id);
        return error is null ? Json(new { success = true }) : BadRequest(error);
    }

    /// <summary>
    /// Returns an empty unit form partial for create.
    /// </summary>
    /// <param name="propertyId">Parent property identifier.</param>
    /// <returns>The unit form partial view.</returns>
    [HttpGet]
    public async Task<IActionResult> CreateUnitModal(int propertyId)
    {
        var model = new UnitFormViewModel { PropertyId = propertyId };
        await PopulateUnitTypesAsync(model);
        return PartialView("_UnitForm", model);
    }

    /// <summary>
    /// Returns a unit form partial populated for edit.
    /// </summary>
    /// <param name="id">Unit identifier.</param>
    /// <returns>The unit form partial, or <see cref="NotFoundResult"/>.</returns>
    [HttpGet]
    public async Task<IActionResult> EditUnitModal(int id)
    {
        var unit = await _properties.GetUnitAsync(id);
        if (unit is null)
        {
            return NotFound();
        }

        var model = unit.ToFormViewModel();
        await PopulateUnitTypesAsync(model, unit.Id);
        return PartialView("_UnitForm", model);
    }

    /// <summary>
    /// Creates or updates a unit under a property.
    /// </summary>
    /// <param name="model">Unit fields from the modal form.</param>
    /// <returns>JSON success with property id, the partial on validation failure, or <see cref="NotFoundResult"/>.</returns>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveUnit(UnitFormViewModel model)
    {
        await PopulateUnitTypesAsync(model, model.Id);

        var (valid, unitTypeError) = await _properties.ValidateUnitTypeAsync(model.UnitTypeId, model.Id);
        if (!valid)
        {
            ModelState.AddModelError(nameof(model.UnitTypeId), unitTypeError!);
        }

        if (!ModelState.IsValid)
        {
            return PartialView("_UnitForm", model);
        }

        var unit = model.Id is int id and > 0
            ? await _properties.GetUnitAsync(id) ?? new Unit { PropertyId = model.PropertyId }
            : new Unit { PropertyId = model.PropertyId };

        if (model.Id is int existingId and > 0 && unit.Id == 0)
        {
            return NotFound();
        }

        unit.ApplyForm(model);

        await _properties.SaveUnitAsync(unit);
        return Json(new { success = true, propertyId = model.PropertyId });
    }

    /// <summary>
    /// Deletes a unit when it has no blocking leases or applications.
    /// </summary>
    /// <param name="id">Unit identifier.</param>
    /// <returns>JSON success with property id, or a bad request with an error message.</returns>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUnit(int id)
    {
        var (success, error, propertyId) = await _properties.TryDeleteUnitAsync(id);
        return success ? Json(new { success = true, propertyId }) : BadRequest(error);
    }

    private async Task PopulateUnitTypesAsync(UnitFormViewModel model, int? existingUnitId = null)
    {
        var types = await _properties.GetSelectableUnitTypesAsync(existingUnitId);
        model.UnitTypeOptions = types.Select(t => t.ToOptionViewModel()).ToList();
    }
}
