using RentalApplications.Domain.Entities;

namespace RentalApplications.Domain;

/// <summary>
/// Rules for applicant edits, submission, and withdrawal by application status.
/// </summary>
public static class ApplicationPolicy
{
    /// <summary>
    /// Returns whether the status is a final outcome (approved, denied, or withdrawn).
    /// </summary>
    public static bool IsTerminal(ApplicationStatus status) =>
        status is ApplicationStatus.Approved or ApplicationStatus.Denied or ApplicationStatus.Withdrawn;

    /// <summary>
    /// Returns whether the applicant may change application content.
    /// </summary>
    public static bool CanApplicantEdit(ApplicationStatus status) =>
        status is ApplicationStatus.Draft or ApplicationStatus.Returned;

    /// <summary>
    /// Returns whether all wizard sections are complete and the application may be submitted.
    /// </summary>
    public static bool CanSubmit(RentalApplication application) =>
        CanApplicantEdit(application.Status)
        && application.ApplicantInformationSaved
        && application.ResidenceHistorySaved;

    /// <summary>
    /// Returns whether the applicant may withdraw the application.
    /// </summary>
    public static bool CanWithdraw(ApplicationStatus status) =>
        status is ApplicationStatus.Draft or ApplicationStatus.Submitted or ApplicationStatus.Returned;
}
