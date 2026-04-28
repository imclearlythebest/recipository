namespace Website.Models;

public class Recipe: Content
{
    public string Title { get; set; } = null!;
    public ICollection<RecipeReview> Reviews { get; set; } = [];
    public ICollection<RecipeReviewRequest> ReviewRequests { get; set; } = [];
    public ICollection<Collection> Collections { get; set; } = [];
    
}