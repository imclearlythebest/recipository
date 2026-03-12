namespace Website.Models;

public class Ingredient
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }

    public float Calories { get; set; }
    public Ingredient()
    {
        Name = string.Empty;
        Description = string.Empty;
        ImageUrl = string.Empty;
        Calories = 0;
    }
    public Ingredient(string name, string description, string imageUrl, float calories)
    {
        this.Name = name;
        this.Description = description;
        this.ImageUrl = imageUrl;
        this.Calories = calories;
    }
}