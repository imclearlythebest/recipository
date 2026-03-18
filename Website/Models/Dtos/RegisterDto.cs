using System.ComponentModel.DataAnnotations;

namespace Website.Models.Dtos;

public class RegisterDto
{
    [Required]
    public string DisplayName { get; set; } = string.Empty;
    [Required]
    public string Username { get; set; } = string.Empty;
    [EmailAddress]
    [Required]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
    [Compare("Password")]
    [Required]
    public string ConfirmPassword { get; set; } = string.Empty;

}