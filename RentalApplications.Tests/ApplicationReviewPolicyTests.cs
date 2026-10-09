using FluentAssertions;
using RentalApplications.Domain;
using RentalApplications.Domain.Entities;

namespace RentalApplications.Tests;

[TestClass]
public class ApplicationReviewPolicyTests
{
    [DataTestMethod]
    [DataRow(ReviewOutcome.Approve, ApplicationStatus.Approved)]
    [DataRow(ReviewOutcome.Return, ApplicationStatus.Returned)]
    [DataRow(ReviewOutcome.Deny, ApplicationStatus.Denied)]
    public void GetTargetStatus_maps_outcomes(ReviewOutcome outcome, ApplicationStatus expected)
    {
        // Act
        var status = ApplicationReviewPolicy.GetTargetStatus(outcome);

        // Assert
        status.Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow(ReviewOutcome.Approve, false)]
    [DataRow(ReviewOutcome.Return, true)]
    [DataRow(ReviewOutcome.Deny, true)]
    public void RequiresComment_for_return_and_deny_only(ReviewOutcome outcome, bool expected)
    {
        // Act
        var required = ApplicationReviewPolicy.RequiresComment(outcome);

        // Assert
        required.Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow(ApplicationStatus.Submitted, true)]
    [DataRow(ApplicationStatus.Draft, false)]
    [DataRow(ApplicationStatus.Returned, false)]
    [DataRow(ApplicationStatus.Approved, false)]
    public void CanReview_only_when_submitted(ApplicationStatus status, bool expected)
    {
        // Act
        var canReview = ApplicationReviewPolicy.CanReview(status);

        // Assert
        canReview.Should().Be(expected);
    }
}
