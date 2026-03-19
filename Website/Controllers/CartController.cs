using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website.Data;
using Website.Models;

namespace Website.Controllers;

public class CartController : Controller
{
    private readonly AppDbContext _context;
    // Static list to persist cart data during the session
    private static List<CartItem> _cart = new List<CartItem>();

    public CartController(AppDbContext context) => _context = context;

    public IActionResult Index() => View(_cart);

    [HttpPost]
    public async Task<IActionResult> AddToCart(int id)
    {
        var item = await _context.Ingredients.OfType<ShopItem>()
                                 .FirstOrDefaultAsync(x => x.Id == id);
        
        if (item == null) return Content("Error: Item not found");

        var existing = _cart.FirstOrDefault(x => x.ShopItemId == id);
        if (existing != null) {
            existing.Quantity += 1;
        } else {
            _cart.Add(new CartItem { ShopItemId = id, Product = item, Quantity = 1 });
        }

        return Content($@"
            <div id='cart-toast' style='position:fixed; top:20px; right:20px; background:#4CAF50; color:white; padding:15px 25px; border-radius:8px; z-index:9999;'>
                <strong>Success!</strong> {item.Name} added to cart.
            </div>");
    }

    [HttpPost]
    public IActionResult UpdateQuantity(int id, int change)
    {
        var item = _cart.FirstOrDefault(x => x.ShopItemId == id);
        if (item != null)
        {
            item.Quantity += change;

            if (item.Quantity <= 0)
            {
                _cart.Remove(item);
            }
        }
        
        // Redirecting back to Index refreshes the table with new totals
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Remove(int id)
    {
        _cart.RemoveAll(x => x.ShopItemId == id);
        return Content(""); 
    }
}