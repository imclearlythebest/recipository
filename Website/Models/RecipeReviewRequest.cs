namespace Website.Models;

public class RecipeReviewRequest
{
    public int Id { get; set; }
    public string ApplicationUserId { get; set; } = string.Empty;
    public int RecipeId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? User { get; set; }
    public Recipe? Recipe { get; set; }
}