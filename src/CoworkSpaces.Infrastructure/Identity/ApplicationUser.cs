using Microsoft.AspNetCore.Identity;

namespace CoworkSpaces.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
