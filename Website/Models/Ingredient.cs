namespace Website.Models;

public class Ingredient
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }

    public float Calories { get; set; }
    public Ingredient(string name, string desc, string imageUrl, float calories)
    {
        Name = name;
        Description = desc;
        ImageUrl = imageUrl;
        Calories = calories;
    }
}