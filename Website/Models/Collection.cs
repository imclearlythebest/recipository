namespace Website.Models;
public class Collection
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }

    public List<Ingredient> Ingredients { get; set; }

    public Collection()
    {
        Name = string.Empty;
        Description = string.Empty;
        ImageUrl = string.Empty;
        Ingredients = new List<Ingredient>();
    }
    public Collection(string name, string description, string imageUrl, List<Ingredient> ingredients)
    {
        this.Name = name;
        this.Description = description;
        this.ImageUrl = imageUrl;
        this.Ingredients = ingredients;
    }
}