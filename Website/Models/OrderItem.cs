namespace Website.Models;

public class OrderItem
{
  public int Id { get; set; }
  public int OrderId { get; set; }
  public int ShopItemId { get; set; }
  public int Quantity { get; set; }
  public float PriceAtPurchase { get; set; }
  
  public ShopItem? Product { get; set; }
}