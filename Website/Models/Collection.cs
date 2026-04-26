namespace Website.Models;

public class Collection
{
    public int Id { get; set; }
    public Guid ApplicationUserId { get; set; }
    public Guid RecipeId { get; set; }
    public string Name { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public ICollection<Recipe> Recipes { get; set; } = new HashSet<Recipe>();
}