using System.ComponentModel.DataAnnotations;

namespace RentalApplications.Models.Applications;

public class ResidenceFormViewModel
{
    public int ApplicationId { get; set; }
    public int? Id { get; set; }

    [Required, StringLength(500)]
    public string Address { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string LandlordName { get; set; } = string.Empty;

    [Required, Phone]
    public string LandlordPhone { get; set; } = string.Empty;

    [Required]
    public DateOnly MoveInDate { get; set; }

    public DateOnly? MoveOutDate { get; set; }
}
