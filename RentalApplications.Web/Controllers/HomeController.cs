using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RentalApplications.Domain;
using RentalApplications.Models;

namespace RentalApplications.Controllers;

/// <summary>
/// Landing page and error handling for the site.
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeController"/> class.
    /// </summary>
    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Routes signed-in users to their primary area or shows the public landing page.
    /// </summary>
    /// <returns>Redirect for known roles, otherwise the home view.</returns>
    public IActionResult Index()
    {
        if (User.IsInRole(AppRoles.PropertyManager))
        {
            return RedirectToAction("Index", "Properties");
        }

        if (User.IsInRole(AppRoles.Applicant))
        {
            return RedirectToAction("Index", "AvailableUnits");
        }

        return View();
    }

    /// <summary>
    /// Displays a diagnostic error page (non-cached).
    /// </summary>
    /// <returns>The error view with a request correlation id.</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        _logger.LogError("Error page displayed for request {RequestId}", requestId);
        return View(new ErrorViewModel { RequestId = requestId });
    }
}
