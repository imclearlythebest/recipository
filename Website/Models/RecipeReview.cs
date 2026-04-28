namespace Website.Models;

public class RecipeReview
{
    public int Id { get; set; }
    public string ApplicationUserId { get; set; } = string.Empty;
    public int RecipeId { get; set; }
    public int Rating { get; set; }
    public string ReviewText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? User { get; set; }
    public Recipe? Recipe { get; set; }
}