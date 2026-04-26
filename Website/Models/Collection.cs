namespace Website.Models;

public class Collection
{
    public int Id { get; set; }
    public string ApplicationUserId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public ICollection<Recipe> Recipes { get; set; } = new HashSet<Recipe>();
}