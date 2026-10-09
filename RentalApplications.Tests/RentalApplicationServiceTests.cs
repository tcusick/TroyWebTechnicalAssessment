using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using RentalApplications.Data;
using RentalApplications.Domain;
using RentalApplications.Domain.Entities;
using RentalApplications.Services;

namespace RentalApplications.Tests;

[TestClass]
public class RentalApplicationServiceTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task<(AppDbContext Db, int UnitId, string ApplicantId, string ManagerId)> SeedAsync()
    {
        var db = CreateContext();
        var applicant = new ApplicationUser { Id = "applicant-1", UserName = "a@test.com", Email = "a@test.com", DisplayName = "Applicant" };
        var manager = new ApplicationUser { Id = "manager-1", UserName = "m@test.com", Email = "m@test.com", DisplayName = "Manager" };
        db.Users.AddRange(applicant, manager);

        var property = new Property { Name = "Test", StreetAddress = "1 Main", City = "City", State = "ST", PostalCode = "12345" };
        db.Properties.Add(property);
        await db.SaveChangesAsync();

        var unitType = new UnitType { Name = "One Bedroom", IsActive = true };
        db.UnitTypes.Add(unitType);
        await db.SaveChangesAsync();

        var unit = new Unit { PropertyId = property.Id, UnitNumber = "101", Bedrooms = 1, MonthlyRent = 1000, UnitTypeId = unitType.Id };
        db.Units.Add(unit);
        await db.SaveChangesAsync();

        return (db, unit.Id, applicant.Id, manager.Id);
    }

    [TestMethod]
    public async Task TryCreate_fails_when_unit_has_active_lease()
    {
        // Arrange
        var (db, unitId, applicantId, _) = await SeedAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var otherApp = new RentalApplication
        {
            UnitId = unitId,
            ApplicantUserId = applicantId,
            Status = ApplicationStatus.Approved,
            ApplicantName = "Applicant",
            Email = "a@test.com"
        };
        db.RentalApplications.Add(otherApp);
        await db.SaveChangesAsync();
        db.Leases.Add(new Lease
        {
            UnitId = unitId,
            RentalApplicationId = otherApp.Id,
            StartDate = today.AddMonths(-1),
            EndDate = today.AddMonths(11)
        });
        await db.SaveChangesAsync();
        var service = new RentalApplicationService(db, new LeaseAvailabilityService(), NullLogger<RentalApplicationService>.Instance);

        // Act
        var result = await service.TryCreateAsync(unitId, applicantId, "Applicant", "a@test.com");

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("no longer available");
    }

    [TestMethod]
    public async Task TrySubmit_fails_when_unit_has_active_lease()
    {
        // Arrange
        var (db, unitId, applicantId, _) = await SeedAsync();
        var service = new RentalApplicationService(db, new LeaseAvailabilityService(), NullLogger<RentalApplicationService>.Instance);
        var create = await service.TryCreateAsync(unitId, applicantId, "Applicant", "a@test.com");
        var app = await service.GetApplicationDetailAsync(create.ApplicationId);
        app!.ApplicantInformationSaved = true;
        app.ResidenceHistorySaved = true;
        await db.SaveChangesAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        db.Leases.Add(new Lease
        {
            UnitId = unitId,
            RentalApplicationId = app.Id,
            StartDate = today,
            EndDate = today.AddMonths(12)
        });
        await db.SaveChangesAsync();

        // Act
        var submit = await service.TrySubmitAsync(app, applicantId);

        // Assert
        submit.Success.Should().BeFalse();
        submit.Error.Should().Contain("no longer available");
    }

    [TestMethod]
    public async Task TrySubmit_succeeds_and_sets_submitted_status()
    {
        // Arrange
        var (db, unitId, applicantId, _) = await SeedAsync();
        var service = new RentalApplicationService(db, new LeaseAvailabilityService(), NullLogger<RentalApplicationService>.Instance);
        var create = await service.TryCreateAsync(unitId, applicantId, "Applicant", "a@test.com");
        var app = await service.GetApplicationDetailAsync(create.ApplicationId);
        app!.ApplicantInformationSaved = true;
        app.ResidenceHistorySaved = true;
        await db.SaveChangesAsync();

        // Act
        var submit = await service.TrySubmitAsync(app, applicantId);

        // Assert
        submit.Success.Should().BeTrue();
        app.Status.Should().Be(ApplicationStatus.Submitted);
    }

    [TestMethod]
    public async Task TryReview_approve_creates_lease()
    {
        // Arrange
        var (db, unitId, applicantId, managerId) = await SeedAsync();
        var service = new RentalApplicationService(db, new LeaseAvailabilityService(), NullLogger<RentalApplicationService>.Instance);
        var create = await service.TryCreateAsync(unitId, applicantId, "Applicant", "a@test.com");
        var app = await service.GetApplicationDetailAsync(create.ApplicationId);
        app!.ApplicantInformationSaved = true;
        app.ResidenceHistorySaved = true;
        app.Status = ApplicationStatus.Submitted;
        await db.SaveChangesAsync();
        var start = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var review = await service.TryReviewAsync(app.Id, ReviewOutcome.Approve, null, start, managerId);

        // Assert
        review.Success.Should().BeTrue();
        var lease = await db.Leases.SingleAsync(l => l.RentalApplicationId == app.Id);
        lease.StartDate.Should().Be(start);
        lease.EndDate.Should().Be(start.AddMonths(LeaseTerms.StandardLeaseMonths));
    }

    [TestMethod]
    public async Task TryReview_approve_fails_when_another_active_lease_exists()
    {
        // Arrange
        var (db, unitId, applicantId, managerId) = await SeedAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var blockingApp = new RentalApplication
        {
            UnitId = unitId,
            ApplicantUserId = applicantId,
            Status = ApplicationStatus.Approved,
            ApplicantName = "Applicant",
            Email = "a@test.com"
        };
        db.RentalApplications.Add(blockingApp);
        await db.SaveChangesAsync();
        db.Leases.Add(new Lease
        {
            UnitId = unitId,
            RentalApplicationId = blockingApp.Id,
            StartDate = today.AddMonths(-1),
            EndDate = today.AddMonths(11)
        });
        var submittedApp = new RentalApplication
        {
            UnitId = unitId,
            ApplicantUserId = applicantId,
            Status = ApplicationStatus.Submitted,
            ApplicantName = "Applicant",
            Email = "a@test.com"
        };
        db.RentalApplications.Add(submittedApp);
        await db.SaveChangesAsync();
        var service = new RentalApplicationService(db, new LeaseAvailabilityService(), NullLogger<RentalApplicationService>.Instance);

        // Act
        var review = await service.TryReviewAsync(submittedApp.Id, ReviewOutcome.Approve, null, today, managerId);

        // Assert
        review.Success.Should().BeFalse();
        review.Error.Should().Contain("active lease");
    }
}
