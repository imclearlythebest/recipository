namespace Website.Models;

public class RecipeApproval
{
    public int Id { get; set; }
    
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    
    public string ApprovedByUserId { get; set; } = string.Empty;
    public ApplicationUser ApprovedBy { get; set; } = null!;
    
    public DateTime ReviewedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public int Version { get; set; } = 1;
}
