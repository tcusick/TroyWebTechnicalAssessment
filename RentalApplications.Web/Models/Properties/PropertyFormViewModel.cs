using System.ComponentModel.DataAnnotations;

namespace RentalApplications.Models.Properties;

public class PropertyFormViewModel
{
    public int? Id { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(300)]
    public string StreetAddress { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string State { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string PostalCode { get; set; } = string.Empty;
}
