using Microsoft.AspNetCore.Identity;

namespace Website.Models;

public class ApplicationUser: IdentityUser
{
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public ICollection<Content> Recipes { get; set; } = [];
    public ICollection<ContentVote> Votes { get; set; } = [];
    public ICollection<Follow> Followers { get; set; } = [];
    public ICollection<Follow> Following { get; set; } = [];
    public ICollection<RecipeReviewRequest> ReviewRequests { get; set; } = [];
    public ICollection<RecipeReview> Reviews { get; set; } = [];
    public ICollection<Collection> Collections  { get; set; } = [];
    public ICollection<CartItem> CartItems { get; set; } = [];
}