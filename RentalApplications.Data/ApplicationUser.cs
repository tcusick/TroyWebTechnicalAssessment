using Microsoft.AspNetCore.Identity;

namespace RentalApplications.Data;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
}
