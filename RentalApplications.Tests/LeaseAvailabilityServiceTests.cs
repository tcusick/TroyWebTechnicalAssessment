using FluentAssertions;
using RentalApplications.Domain.Entities;
using RentalApplications.Services;

namespace RentalApplications.Tests;

[TestClass]
public class LeaseAvailabilityServiceTests
{
    private readonly LeaseAvailabilityService _service = new();

    [TestMethod]
    public void Unit_is_unavailable_when_lease_covers_today()
    {
        // Arrange
        var today = new DateOnly(2026, 6, 15);
        var leases = new[]
        {
            new Lease { StartDate = new DateOnly(2026, 1, 1), EndDate = new DateOnly(2026, 12, 31) }
        };

        // Act
        var available = _service.IsUnitAvailableOnDate(leases, today);

        // Assert
        available.Should().BeFalse();
    }

    [TestMethod]
    public void Unit_is_available_when_lease_ended_before_today()
    {
        // Arrange
        var today = new DateOnly(2026, 6, 15);
        var leases = new[]
        {
            new Lease { StartDate = new DateOnly(2024, 1, 1), EndDate = new DateOnly(2025, 12, 31) }
        };

        // Act
        var available = _service.IsUnitAvailableOnDate(leases, today);

        // Assert
        available.Should().BeTrue();
    }

    [TestMethod]
    public void End_date_is_twelve_months_after_start()
    {
        // Arrange
        var start = new DateOnly(2026, 3, 1);

        // Act
        var end = _service.CalculateEndDate(start);

        // Assert
        end.Should().Be(new DateOnly(2027, 3, 1));
    }

    [TestMethod]
    public void Lease_covers_first_and_last_day_of_term()
    {
        // Arrange
        var lease = new Lease { StartDate = new DateOnly(2026, 1, 1), EndDate = new DateOnly(2026, 12, 31) };

        // Act & Assert
        _service.IsUnitAvailableOnDate([lease], new DateOnly(2026, 1, 1)).Should().BeFalse();
        _service.IsUnitAvailableOnDate([lease], new DateOnly(2026, 12, 31)).Should().BeFalse();
        _service.IsUnitAvailableOnDate([lease], new DateOnly(2027, 1, 1)).Should().BeTrue();
    }

    [TestMethod]
    public void HasActiveLeaseOnDate_is_inverse_of_availability()
    {
        // Arrange
        var today = new DateOnly(2026, 6, 1);
        var leases = new[] { new Lease { StartDate = new DateOnly(2026, 1, 1), EndDate = new DateOnly(2026, 12, 31) } };

        // Act
        var hasLease = _service.HasActiveLeaseOnDate(leases, today);
        var available = _service.IsUnitAvailableOnDate(leases, today);

        // Assert
        hasLease.Should().BeTrue();
        available.Should().BeFalse();
    }
}
