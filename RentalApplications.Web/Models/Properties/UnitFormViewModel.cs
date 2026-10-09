using System.ComponentModel.DataAnnotations;

namespace RentalApplications.Models.Properties;

public class UnitFormViewModel
{
    public int? Id { get; set; }
    public int PropertyId { get; set; }

    [Required, StringLength(50)]
    public string UnitNumber { get; set; } = string.Empty;

    [Range(0, 20)]
    public int Bedrooms { get; set; }

    [Range(0, 100000)]
    public decimal MonthlyRent { get; set; }

    [Required]
    public int UnitTypeId { get; set; }

    public IReadOnlyList<UnitTypeOptionViewModel> UnitTypeOptions { get; set; } = Array.Empty<UnitTypeOptionViewModel>();
}

public class UnitTypeOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
