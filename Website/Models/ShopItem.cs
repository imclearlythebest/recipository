namespace Website.Models; 

public class ShopItem
{
    public int Id { get; set; }
    public int IngredientId { get; set; } 
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int Moq { get; set; } 
    public int Increment { get; set; }

    public Ingredient Ingredient { get; set; } = new Ingredient();
}