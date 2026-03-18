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

    var item = await _context.Ingredients.OfType<ShopItem>().FirstOrDefaultAsync(x => x.Id == id);
    
    if (item != null)
    {
      var existing = _cart.FirstOrDefault(x => x.ShopItemId == id);
      if (existing != null) {
        existing.Quantity += 1;
      } else {
        _cart.Add(new CartItem { ShopItemId = id, Product = item, Quantity = 1 });
      }
    }

    return Ok(); 
  }

  [HttpPost]
  public IActionResult Remove(int id)
  {
    _cart.RemoveAll(x => x.ShopItemId == id);

    return Content(""); 
  }
}