using Microsoft.AspNetCore.Identity;

namespace Website.Models;
public class ApplicationUser: IdentityUser
{
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
}