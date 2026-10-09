using FluentAssertions;
using RentalApplications.Domain;
using RentalApplications.Domain.Entities;

namespace RentalApplications.Tests;

[TestClass]
public class UnitTypeSelectionServiceTests
{
    [TestMethod]
    public void Inactive_type_cannot_be_selected_for_new_unit()
    {
        // Arrange
        var inactive = new UnitType { Id = 1, IsActive = false };

        // Act
        var allowed = UnitTypePolicy.CanAssignUnitType(inactive, null);

        // Assert
        allowed.Should().BeFalse();
    }

    [TestMethod]
    public void Inactive_type_remains_on_existing_unit()
    {
        // Arrange
        var inactive = new UnitType { Id = 1, IsActive = false };
        var unit = new Unit { UnitTypeId = 1 };

        // Act
        var allowed = UnitTypePolicy.CanAssignUnitType(inactive, unit);

        // Assert
        allowed.Should().BeTrue();
    }
}
