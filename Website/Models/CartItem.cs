namespace Website.Models;

public class CartItem
{
    public int Id { get; set; }
    public int ShopItemId { get; set; }
    
    public ShopItem? Product { get; set; } 
    
    public int Quantity { get; set; }

    public float Subtotal => (Product?.Price ?? 0) * Quantity;
}