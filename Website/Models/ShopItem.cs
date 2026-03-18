namespace Website.Models;

public class ShopItem
{
  public int Id { get; set; } // Entity Framework will handle this ID
  public string Name { get; set; } = string.Empty;
  public string? Description { get; set; }
  public float Price { get; set; }
  public string? ImageUrl { get; set; }
}