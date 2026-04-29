using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Website.Data;
using Website.Models;

namespace Website.Controllers;

[Authorize]
public class CartController : Controller
{
  private readonly AppDbContext _context;
  private readonly UserManager<ApplicationUser> _userManager;

  public CartController(AppDbContext context, UserManager<ApplicationUser> userManager)
  {
    _context = context;
    _userManager = userManager;
  }

  public async Task<IActionResult> Index()
  {
    var userId = _userManager.GetUserId(User);
    var cartItems = await _context.CartItems
      .Include(c => c.Product)
      .Where(c => c.ApplicationUserId == userId)
      .ToListAsync();
    return View(cartItems);
  }

  [HttpPost]
  public async Task<IActionResult> AddToCart(int id)
  {
    var userId = _userManager.GetUserId(User);
    if (string.IsNullOrEmpty(userId)) return Challenge();

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

    // Update Cart in Database
    var existing = await _context.CartItems
      .FirstOrDefaultAsync(x => x.ShopItemId == id && x.ApplicationUserId == userId);

    if (existing != null) {
      existing.Quantity += 1;
    } else {
      _context.CartItems.Add(new CartItem { 
        ShopItemId = id, 
        ApplicationUserId = userId!, 
        Quantity = 1 
      });
    }

    await _context.SaveChangesAsync();

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
    var userId = _userManager.GetUserId(User);
    var cartItem = await _context.CartItems
      .FirstOrDefaultAsync(x => x.ShopItemId == id && x.ApplicationUserId == userId);
    
    if (cartItem == null) return RedirectToAction("Index");

    var dbItem = await _context.Ingredients.OfType<ShopItem>().FirstOrDefaultAsync(x => x.Id == id);

    if (change > 0)
    {
      if (dbItem != null && dbItem.Stock > 0)
      {
        dbItem.Stock -= 1;
        cartItem.Quantity += 1;
      }
      else if (dbItem != null)
      {
        ViewBag.Toast = $@"
          <div id='cart-toast' hx-swap-oob='true' style='position:fixed; top:20px; right:20px; background:#e74c3c; color:white; padding:15px 25px; border-radius:8px; z-index:9999; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
            <strong>Stock Out!</strong> No more {dbItem.Name} available.
          </div>";
      }
    }
    else if (change < 0)
    {
      if (dbItem != null)
      {
        dbItem.Stock += 1;
      }
      cartItem.Quantity -= 1;
      if (cartItem.Quantity <= 0) 
      {
        _context.CartItems.Remove(cartItem);
      }
    }
    
    await _context.SaveChangesAsync();

    var updatedCart = await _context.CartItems
      .Include(c => c.Product)
      .Where(c => c.ApplicationUserId == userId)
      .ToListAsync();

    return View("Index", updatedCart);
  }

  [HttpPost]
  public async Task<IActionResult> Remove(int id)
  {
    var userId = _userManager.GetUserId(User);
    var cartItem = await _context.CartItems
      .FirstOrDefaultAsync(x => x.ShopItemId == id && x.ApplicationUserId == userId);

    if (cartItem != null)
    {
      var dbItem = await _context.Ingredients.OfType<ShopItem>().FirstOrDefaultAsync(x => x.Id == id);
      if (dbItem != null)
      {
        dbItem.Stock += cartItem.Quantity;
      }
      _context.CartItems.Remove(cartItem);
      await _context.SaveChangesAsync();
    }

    var updatedCart = await _context.CartItems
      .Include(c => c.Product)
      .Where(c => c.ApplicationUserId == userId)
      .ToListAsync();

    return View("Index", updatedCart); 
  }
}