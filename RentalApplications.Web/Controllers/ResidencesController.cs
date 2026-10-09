using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentalApplications.Data;
using RentalApplications.Domain;
using RentalApplications.Domain.Entities;
using RentalApplications.Mapping;
using RentalApplications.Models.Applications;
using RentalApplications.Services.Abstractions;

namespace RentalApplications.Controllers;

/// <summary>
/// Modal CRUD for prior residence entries on a rental application.
/// </summary>
[Authorize(Roles = AppRoles.Applicant)]
public class ResidencesController : Controller
{
    private readonly IResidenceHistoryService _residences;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResidencesController"/> class.
    /// </summary>
    public ResidencesController(IResidenceHistoryService residences, UserManager<ApplicationUser> userManager)
    {
        _residences = residences;
        _userManager = userManager;
    }

    /// <summary>
    /// Returns an empty residence form partial for create.
    /// </summary>
    /// <param name="applicationId">Parent application identifier.</param>
    /// <returns>The residence form partial, or <see cref="ForbidResult"/> when the application is not editable.</returns>
    [HttpGet]
    public async Task<IActionResult> CreateModal(int applicationId)
    {
        if (!await _residences.CanApplicantEditApplicationAsync(applicationId, _userManager.GetUserId(User)!))
        {
            return Forbid();
        }

        return PartialView("_ResidenceForm", new ResidenceFormViewModel { ApplicationId = applicationId });
    }

    /// <summary>
    /// Returns a residence form partial populated for edit.
    /// </summary>
    /// <param name="id">Residence history identifier.</param>
    /// <returns>The residence form partial, or <see cref="ForbidResult"/>.</returns>
    [HttpGet]
    public async Task<IActionResult> EditModal(int id)
    {
        var residence = await _residences.GetAsync(id);
        if (residence is null || !ApplicationPolicy.CanApplicantEdit(residence.RentalApplication.Status))
        {
            return Forbid();
        }

        if (residence.RentalApplication.ApplicantUserId != _userManager.GetUserId(User))
        {
            return Forbid();
        }

        return PartialView("_ResidenceForm", residence.ToFormViewModel());
    }

    /// <summary>
    /// Creates or updates a residence history row for an application.
    /// </summary>
    /// <param name="model">Residence fields from the modal form.</param>
    /// <returns>JSON success with application id, the partial on validation failure, or <see cref="ForbidResult"/>.</returns>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ResidenceFormViewModel model)
    {
        var userId = _userManager.GetUserId(User)!;
        if (!await _residences.CanApplicantEditApplicationAsync(model.ApplicationId, userId))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return PartialView("_ResidenceForm", model);
        }

        var residence = model.Id is int id and > 0
            ? await _residences.GetAsync(id) ?? new ResidenceHistory { RentalApplicationId = model.ApplicationId }
            : new ResidenceHistory { RentalApplicationId = model.ApplicationId };

        if (model.Id is int existingId and > 0 && residence.Id == 0)
        {
            return NotFound();
        }

        residence.ApplyForm(model);

        await _residences.SaveAsync(residence);
        return Json(new { success = true, applicationId = model.ApplicationId });
    }

    /// <summary>
    /// Deletes a residence history row.
    /// </summary>
    /// <param name="id">Residence history identifier.</param>
    /// <returns>JSON success with application id, or <see cref="ForbidResult"/>.</returns>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var residence = await _residences.GetAsync(id);
        if (residence is null || !ApplicationPolicy.CanApplicantEdit(residence.RentalApplication.Status))
        {
            return Forbid();
        }

        if (residence.RentalApplication.ApplicantUserId != _userManager.GetUserId(User))
        {
            return Forbid();
        }

        var (_, applicationId) = await _residences.TryDeleteAsync(id);
        return Json(new { success = true, applicationId });
    }
}
