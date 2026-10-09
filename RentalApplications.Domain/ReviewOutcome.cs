namespace RentalApplications.Domain;

/// <summary>
/// Property manager decision on a submitted application.
/// </summary>
public enum ReviewOutcome
{
    Approve,
    Return,
    Deny
}

/// <summary>
/// Rules for reviewing submitted applications.
/// </summary>
public static class ApplicationReviewPolicy
{
    /// <summary>
    /// Maps a review outcome to the application status to apply, if valid.
    /// </summary>
    public static Entities.ApplicationStatus? GetTargetStatus(ReviewOutcome outcome) =>
        outcome switch
        {
            ReviewOutcome.Approve => Entities.ApplicationStatus.Approved,
            ReviewOutcome.Return => Entities.ApplicationStatus.Returned,
            ReviewOutcome.Deny => Entities.ApplicationStatus.Denied,
            _ => null
        };

    /// <summary>
    /// Returns whether the manager must supply a comment for the outcome.
    /// </summary>
    public static bool RequiresComment(ReviewOutcome outcome) =>
        outcome is ReviewOutcome.Return or ReviewOutcome.Deny;

    /// <summary>
    /// Returns whether an application in the given status may be reviewed.
    /// </summary>
    public static bool CanReview(Entities.ApplicationStatus status) =>
        status == Entities.ApplicationStatus.Submitted;
}
