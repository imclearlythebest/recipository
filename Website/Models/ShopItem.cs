namespace Website.Models;

public class ShopItem: Ingredient
{
    public float Price { get; set; }
    public int Stock { get; set; }
    public int Moq { get; set; }
    public int Increment { get; set; }
    public ShopItem() : base()
    {
        Price = 0;
        Stock = 0;
        Moq = 0;
        Increment = 0;
    }
    public ShopItem(string name, string description, string imageUrl, float calories, float price, int stock, int moq, int increment) : base(name, description, imageUrl, calories)
    {
        this.Price = price;
        this.Stock = stock;
        this.Moq = moq;
        this.Increment = increment;
    }
}