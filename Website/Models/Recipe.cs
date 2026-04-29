namespace Website.Models;

public class Recipe : Content
{
    public string Title { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public string Instructions { get; set; } = string.Empty;
    public int PrepTime { get; set; }
    public int CookTime { get; set; }
    public int ServingSize { get; set; }
    public string Difficulty { get; set; } = "Medium";
    public string Status { get; set; } = "Draft";
    public float EstimatedCalories { get; set; } = 0;
    public DateTime? PublishedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }

    public ICollection<RecipeReview> Reviews { get; set; } = [];
    public ICollection<RecipeReviewRequest> ReviewRequests { get; set; } = [];
    public ICollection<Collection> Collections { get; set; } = [];
    public ICollection<Ingredient> Ingredients { get; set; } = [];
    public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = [];
    public ICollection<RecipeApproval> ApprovalHistory { get; set; } = [];
    public ICollection<RecipeRevenue> Revenues { get; set; } = [];
}