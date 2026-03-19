using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website.Data;
using Website.Models;

namespace Website.Controllers;

public class CartController : Controller
{
  private readonly AppDbContext _context;
  private static List<CartItem> _cart = new List<CartItem>();

  public CartController(AppDbContext context) => _context = context;

  public IActionResult Index() => View(_cart);

  [HttpPost]
  public async Task<IActionResult> AddToCart(int id)
  {
    var item = await _context.Ingredients.OfType<ShopItem>()
                   .FirstOrDefaultAsync(x => x.Id == id);
    
    if (item == null) return Content("Error: Item not found");

    // Stock/Price Validation
    if (item.Price <= 0 || item.Stock <= 0)
    {
      return Content($@"
        <div id='cart-toast' hx-swap-oob='true' style='position:fixed; top:20px; right:20px; background:#e74c3c; color:white; padding:15px 25px; border-radius:8px; z-index:9999; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
          <strong>Stock Out!</strong> {item.Name} is no longer available.
        </div>");
    }

    // Update Database Stock
    item.Stock -= 1;
    await _context.SaveChangesAsync();

    // Update Static Cart
    var existing = _cart.FirstOrDefault(x => x.ShopItemId == id);
    if (existing != null) {
      existing.Quantity += 1;
    } else {
      _cart.Add(new CartItem { ShopItemId = id, Product = item, Quantity = 1 });
    }

    // OOB Response: Updates the toast AND the specific stock label on the marketplace
    return Content($@"
      <div id='cart-toast' hx-swap-oob='true' style='position:fixed; top:20px; right:20px; background:#4CAF50; color:white; padding:15px 25px; border-radius:8px; z-index:9999; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
        <strong>Success!</strong> {item.Name} added to cart.
      </div>
      <small id='stock-{id}' hx-swap-oob='true' style='color: var(--pico-muted-color);'>In Stock: {item.Stock}</small>");
  }

  [HttpPost]
  public async Task<IActionResult> UpdateQuantity(int id, int change)
  {
    var cartItem = _cart.FirstOrDefault(x => x.ShopItemId == id);
    var dbItem = await _context.Ingredients.OfType<ShopItem>().FirstOrDefaultAsync(x => x.Id == id);

    if (cartItem != null && dbItem != null)
    {
      if (change > 0 && dbItem.Stock > 0)
      {
        dbItem.Stock -= 1;
        cartItem.Quantity += 1;
      }
      else if (change < 0)
      {
        dbItem.Stock += 1;
        cartItem.Quantity -= 1;
        if (cartItem.Quantity <= 0) _cart.Remove(cartItem);
      }
      await _context.SaveChangesAsync();
    }
    return View("Index", _cart);
  }

  [HttpPost]
  public async Task<IActionResult> Remove(int id)
  {
    var cartItem = _cart.FirstOrDefault(x => x.ShopItemId == id);
    if (cartItem != null)
    {
      var dbItem = await _context.Ingredients.OfType<ShopItem>().FirstOrDefaultAsync(x => x.Id == id);
      if (dbItem != null)
      {
        dbItem.Stock += cartItem.Quantity;
        await _context.SaveChangesAsync();
      }
      _cart.Remove(cartItem);
    }
    return View("Index", _cart); 
  }

  public static List<CartItem> GetCartItems() => _cart;
  public static void ClearCart() => _cart.Clear();
}