using FluentAssertions;
using RentalApplications.Domain;
using RentalApplications.Domain.Entities;

namespace RentalApplications.Tests;

[TestClass]
public class ApplicationPolicyTests
{
    [DataTestMethod]
    [DataRow(ApplicationStatus.Approved, true)]
    [DataRow(ApplicationStatus.Denied, true)]
    [DataRow(ApplicationStatus.Withdrawn, true)]
    [DataRow(ApplicationStatus.Draft, false)]
    [DataRow(ApplicationStatus.Submitted, false)]
    [DataRow(ApplicationStatus.Returned, false)]
    public void IsTerminal(ApplicationStatus status, bool expected)
    {
        // Act
        var result = ApplicationPolicy.IsTerminal(status);

        // Assert
        result.Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow(ApplicationStatus.Draft, true)]
    [DataRow(ApplicationStatus.Returned, true)]
    [DataRow(ApplicationStatus.Submitted, false)]
    [DataRow(ApplicationStatus.Approved, false)]
    public void CanApplicantEdit(ApplicationStatus status, bool expected)
    {
        // Act
        var result = ApplicationPolicy.CanApplicantEdit(status);

        // Assert
        result.Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow(ApplicationStatus.Draft, true)]
    [DataRow(ApplicationStatus.Submitted, true)]
    [DataRow(ApplicationStatus.Returned, true)]
    [DataRow(ApplicationStatus.Approved, false)]
    [DataRow(ApplicationStatus.Denied, false)]
    [DataRow(ApplicationStatus.Withdrawn, false)]
    public void CanWithdraw(ApplicationStatus status, bool expected)
    {
        // Act
        var result = ApplicationPolicy.CanWithdraw(status);

        // Assert
        result.Should().Be(expected);
    }

    [TestMethod]
    public void CanSubmit_requires_editable_status_and_both_sections()
    {
        // Arrange
        var app = new RentalApplication
        {
            Status = ApplicationStatus.Draft,
            ApplicantInformationSaved = true,
            ResidenceHistorySaved = true
        };

        // Act & Assert
        ApplicationPolicy.CanSubmit(app).Should().BeTrue();

        app.Status = ApplicationStatus.Submitted;
        ApplicationPolicy.CanSubmit(app).Should().BeFalse();

        app.Status = ApplicationStatus.Returned;
        app.ResidenceHistorySaved = false;
        ApplicationPolicy.CanSubmit(app).Should().BeFalse();
    }
}
