using RentalApplications.Domain;
using RentalApplications.Domain.Entities;
using RentalApplications.Services.Models;

namespace RentalApplications.Services.Abstractions;

/// <summary>
/// Application lifecycle: listing, wizard progress, submission, review, and status changes.
/// </summary>
public interface IRentalApplicationService
{
    /// <summary>
    /// Lists applications for an applicant or all applications for a property manager, with optional filters.
    /// </summary>
    Task<IReadOnlyList<ApplicationListRow>> ListAsync(
        ApplicationStatus? status,
        int? propertyId,
        string? applicantUserId,
        bool isPropertyManager,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns properties used to populate the manager application list filter.
    /// </summary>
    Task<IReadOnlyList<Property>> GetPropertiesForFilterAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a draft application for a unit when the unit is available and the applicant has no open application for it.
    /// </summary>
    Task<(bool Success, string? Error, int ApplicationId)> TryCreateAsync(
        int unitId,
        string userId,
        string displayName,
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads an application with unit, property, and residence history for display and editing.
    /// </summary>
    Task<RentalApplication?> GetApplicationDetailAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists the wizard section the applicant is viewing.
    /// </summary>
    Task SetCurrentSectionAsync(RentalApplication application, ApplicationSection section, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves applicant information and marks that section complete on the application.
    /// </summary>
    Task SaveApplicantInformationAsync(
        RentalApplication application,
        string name,
        string phone,
        string email,
        string address,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the residence history section complete after validation in the UI.
    /// </summary>
    Task MarkResidenceHistoryCompleteAsync(RentalApplication application, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes application status and records a history entry.
    /// </summary>
    Task ChangeStatusAsync(
        RentalApplication application,
        ApplicationStatus toStatus,
        string changedByUserId,
        string? comment,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates and submits a draft or returned application.
    /// </summary>
    Task<(bool Success, string? Error)> TrySubmitAsync(
        RentalApplication application,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a manager review (approve, return, or deny) and creates a lease on approval.
    /// </summary>
    Task<(bool Success, string? Error)> TryReviewAsync(
        int applicationId,
        ReviewOutcome outcome,
        string? comment,
        DateOnly leaseStartDate,
        string reviewerUserId,
        CancellationToken cancellationToken = default);
}
