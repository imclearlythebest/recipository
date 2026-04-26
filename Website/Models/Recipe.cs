namespace Website.Models;

public class Recipe: Content
{
    public ICollection<Collection> Collections { get; set; } = [];
    
}