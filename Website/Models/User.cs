namespace Website.Models;
public class User
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public string? DisplayName { get; set; }
    public required string Email { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public required string Password { get; set; }
    public bool IsAdmin { get; set; } = false;
}