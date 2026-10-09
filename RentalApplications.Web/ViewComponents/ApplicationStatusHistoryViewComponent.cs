using Microsoft.AspNetCore.Mvc;
using RentalApplications.Services.Abstractions;

namespace RentalApplications.ViewComponents;

/// <summary>
/// Renders status change history for a rental application (property managers).
/// </summary>
public class ApplicationStatusHistoryViewComponent : ViewComponent
{
    private readonly IApplicationStatusHistoryService _history;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationStatusHistoryViewComponent"/> class.
    /// </summary>
    public ApplicationStatusHistoryViewComponent(IApplicationStatusHistoryService history)
    {
        _history = history;
    }

    /// <summary>
    /// Loads status history for the application and returns the default view.
    /// </summary>
    /// <param name="applicationId">Application identifier.</param>
    public async Task<IViewComponentResult> InvokeAsync(int applicationId)
    {
        var items = await _history.GetHistoryAsync(applicationId);
        return View(items);
    }
}
