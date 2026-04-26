namespace Website.Models;

public class Recipe: Content
{
    public string Title { get; set; } = null!;
    public ICollection<Collection> Collections { get; set; } = [];
    
}