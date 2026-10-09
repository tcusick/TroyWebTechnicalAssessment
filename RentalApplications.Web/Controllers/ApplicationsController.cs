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
/// Rental application list, multi-step edit wizard, withdrawal, and property-manager review.
/// </summary>
[Authorize]
public class ApplicationsController : Controller
{
    private readonly IRentalApplicationService _applications;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationsController"/> class.
    /// </summary>
    public ApplicationsController(IRentalApplicationService applications, UserManager<ApplicationUser> userManager)
    {
        _applications = applications;
        _userManager = userManager;
    }

    /// <summary>
    /// Lists applications visible to the signed-in user, with optional manager filters.
    /// </summary>
    /// <param name="filter">Optional status and property filters (property filter is for managers).</param>
    /// <returns>The application list view.</returns>
    [HttpGet]
    public async Task<IActionResult> Index(ApplicationListFilterViewModel filter)
    {
        var isManager = User.IsInRole(AppRoles.PropertyManager);
        var userId = _userManager.GetUserId(User);
        var items = await _applications.ListAsync(
            filter.Status,
            filter.PropertyId,
            userId,
            isManager);

        ViewBag.Properties = await _applications.GetPropertiesForFilterAsync();
        ViewBag.Filter = filter;
        return View(items.Select(i => i.ToListItemViewModel()).ToList());
    }

    /// <summary>
    /// Starts a new draft application for the given unit.
    /// </summary>
    /// <param name="unitId">Unit to apply for.</param>
    /// <returns>Redirects to the edit wizard on success, or back to available units with an error message.</returns>
    [Authorize(Roles = AppRoles.Applicant)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int unitId)
    {
        var user = await _userManager.GetUserAsync(User);
        var result = await _applications.TryCreateAsync(
            unitId,
            user!.Id,
            user.DisplayName,
            user.Email ?? string.Empty);

        if (!result.Success)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(AvailableUnitsController.Index), "AvailableUnits");
        }

        return RedirectToAction(nameof(Edit), new { id = result.ApplicationId });
    }

    /// <summary>
    /// Shows the application edit wizard for applicants and read-only detail for managers.
    /// </summary>
    /// <param name="id">Application identifier.</param>
    /// <returns>The edit view, or <see cref="NotFoundResult"/> / <see cref="ForbidResult"/> when not allowed.</returns>
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var application = await _applications.GetApplicationDetailAsync(id);
        if (application is null)
        {
            return NotFound();
        }

        if (!await CanAccessAsync(application))
        {
            return Forbid();
        }

        return View(application.ToEditFormViewModel(FormPresentation()));
    }

    /// <summary>
    /// Handles wizard navigation (back, continue, submit) for an application.
    /// </summary>
    /// <param name="model">Posted form state including the <see cref="RentalApplicationFormViewModel.Command"/> value.</param>
    /// <returns>The appropriate view, redirect, or error result.</returns>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(RentalApplicationFormViewModel model)
    {
        var application = await _applications.GetApplicationDetailAsync(model.Id);
        if (application is null)
        {
            return NotFound();
        }

        if (!await CanAccessAsync(application))
        {
            return Forbid();
        }

        var readOnly = !ApplicationPolicy.CanApplicantEdit(application.Status) || User.IsInRole(AppRoles.PropertyManager);
        if (readOnly && model.Command is not null)
        {
            return BadRequest();
        }

        switch (model.Command?.ToLowerInvariant())
        {
            case "back":
                model.CurrentSection = model.CurrentSection switch
                {
                    ApplicationSection.ResidenceHistory => ApplicationSection.ApplicantInformation,
                    ApplicationSection.Summary => ApplicationSection.ResidenceHistory,
                    _ => model.CurrentSection
                };
                await _applications.SetCurrentSectionAsync(application, model.CurrentSection);
                return View(application.ToEditFormViewModel(FormPresentation(model.CurrentSection)));

            case "continue":
                return await HandleContinueAsync(application, model);

            case "submit":
                return await HandleSubmitAsync(application, model);

            default:
                return View(application.ToEditFormViewModel(FormPresentation(application.CurrentSection)));
        }
    }

    /// <summary>
    /// Withdraws a draft, submitted, or returned application.
    /// </summary>
    /// <param name="id">Application identifier.</param>
    /// <returns>Redirects to the application list.</returns>
    [Authorize(Roles = AppRoles.Applicant)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(int id)
    {
        var application = await _applications.GetApplicationDetailAsync(id);
        if (application is null || application.ApplicantUserId != _userManager.GetUserId(User))
        {
            return Forbid();
        }

        if (!ApplicationPolicy.CanWithdraw(application.Status))
        {
            return BadRequest();
        }

        await _applications.ChangeStatusAsync(
            application,
            ApplicationStatus.Withdrawn,
            _userManager.GetUserId(User)!,
            "Withdrawn by applicant.");
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Returns the property-manager review modal partial for a submitted application.
    /// </summary>
    /// <param name="id">Application identifier.</param>
    /// <returns>The review form partial view, or <see cref="NotFoundResult"/> when review is not allowed.</returns>
    [Authorize(Roles = AppRoles.PropertyManager)]
    [HttpGet]
    public async Task<IActionResult> ReviewModal(int id)
    {
        var application = await _applications.GetApplicationDetailAsync(id);
        if (application is null || !ApplicationReviewPolicy.CanReview(application.Status))
        {
            return NotFound();
        }

        return PartialView("_ReviewForm", new ReviewApplicationViewModel
        {
            ApplicationId = application.Id,
            ApplicantName = application.ApplicantName,
            UnitSummary = $"{application.Unit.Property.Name} — Unit {application.Unit.UnitNumber}",
            LeaseStartDate = DateOnly.FromDateTime(DateTime.UtcNow)
        });
    }

    /// <summary>
    /// Approves, returns, or denies a submitted application and optionally creates a lease on approval.
    /// </summary>
    /// <param name="model">Review outcome, comment, and lease start date.</param>
    /// <returns>JSON success payload, or the review partial with validation errors.</returns>
    [Authorize(Roles = AppRoles.PropertyManager)]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(ReviewApplicationViewModel model)
    {
        if (ApplicationReviewPolicy.RequiresComment(model.Outcome) && string.IsNullOrWhiteSpace(model.Comment))
        {
            ModelState.AddModelError(nameof(model.Comment), "A comment is required for this outcome.");
        }

        if (!ModelState.IsValid)
        {
            return PartialView("_ReviewForm", model);
        }

        var result = await _applications.TryReviewAsync(
            model.ApplicationId,
            model.Outcome,
            model.Comment,
            model.LeaseStartDate,
            _userManager.GetUserId(User)!);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return PartialView("_ReviewForm", model);
        }

        return Json(new { success = true });
    }

    private async Task<IActionResult> HandleContinueAsync(RentalApplication application, RentalApplicationFormViewModel model)
    {
        if (!ApplicationPolicy.CanApplicantEdit(application.Status))
        {
            return Forbid();
        }

        switch (model.CurrentSection)
        {
            case ApplicationSection.ApplicantInformation:
                if (!TryValidateModel(model.ApplicantInformation, prefix: nameof(RentalApplicationFormViewModel.ApplicantInformation)))
                {
                    model.CurrentSection = ApplicationSection.ApplicantInformation;
                    model.RefreshEditFormShell(application, FormPresentation());
                    return View(model);
                }

                await _applications.SaveApplicantInformationAsync(
                    application,
                    model.ApplicantInformation.ApplicantName,
                    model.ApplicantInformation.Phone,
                    model.ApplicantInformation.Email,
                    model.ApplicantInformation.CurrentAddress);
                break;

            case ApplicationSection.ResidenceHistory:
                if (!application.ResidenceHistories.Any())
                {
                    ModelState.AddModelError($"{nameof(RentalApplicationFormViewModel.ResidenceHistory)}.Items", "Add at least one prior residence.");
                    model.CurrentSection = ApplicationSection.ResidenceHistory;
                    model.RefreshEditFormShell(application, FormPresentation());
                    return View(model);
                }

                await _applications.MarkResidenceHistoryCompleteAsync(application);
                break;

            default:
                await _applications.SetCurrentSectionAsync(application, ApplicationSection.Summary);
                break;
        }

        return View(application.ToEditFormViewModel(FormPresentation(application.CurrentSection)));
    }

    private async Task<IActionResult> HandleSubmitAsync(RentalApplication application, RentalApplicationFormViewModel model)
    {
        if (!ApplicationPolicy.CanApplicantEdit(application.Status))
        {
            return Forbid();
        }

        if (model.CurrentSection != ApplicationSection.Summary)
        {
            return BadRequest();
        }

        var result = await _applications.TrySubmitAsync(application, _userManager.GetUserId(User)!);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            model.RefreshEditFormShell(application, FormPresentation());
            return View(model);
        }

        TempData["Message"] = "Application submitted.";
        return RedirectToAction(nameof(Index));
    }

    private ApplicationFormPresentation FormPresentation(ApplicationSection? section = null) =>
        new(section, User.IsInRole(AppRoles.Applicant), User.IsInRole(AppRoles.PropertyManager));

    private async Task<bool> CanAccessAsync(RentalApplication application)
    {
        if (User.IsInRole(AppRoles.PropertyManager))
        {
            return true;
        }

        return application.ApplicantUserId == _userManager.GetUserId(User);
    }
}
