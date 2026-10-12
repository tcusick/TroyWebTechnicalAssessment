using System.ComponentModel.DataAnnotations;
using RentalApplications.Validation;

namespace RentalApplications.Models.Properties;

public class UnitFormViewModel
{
    public int? Id { get; set; }
    public int PropertyId { get; set; }

    [Required, StringLength(50, MinimumLength = 1), Display(Name = "Unit number"), RegularExpression(ValidationConstants.NonWhitespacePattern, ErrorMessage = ValidationConstants.NonWhitespaceErrorMessage)]
    public string UnitNumber { get; set; } = string.Empty;

    [Range(0, 20, ErrorMessage = "Bedrooms must be between 0 and 20.")]
    public int Bedrooms { get; set; }

    [Range(typeof(decimal), "0.01", "100000", ErrorMessage = "Monthly rent must be between $0.01 and $100,000.")]
    [Display(Name = "Monthly rent")]
    public decimal MonthlyRent { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Select a unit type.")]
    [Display(Name = "Unit type")]
    public int UnitTypeId { get; set; }

    public IReadOnlyList<UnitTypeOptionViewModel> UnitTypeOptions { get; set; } = Array.Empty<UnitTypeOptionViewModel>();
}

public class UnitTypeOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
