using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Website.Data;
using Website.Models;
using Website.Services;

namespace Website.Controllers;

[Authorize]
public class CheckoutController : Controller
{
  private readonly AppDbContext _context;
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly IRevenueService _revenueService;

  public CheckoutController(AppDbContext context, UserManager<ApplicationUser> userManager, IRevenueService revenueService)
  {
    _context = context;
    _userManager = userManager;
    _revenueService = revenueService;
  }

  public async Task<IActionResult> Index()
  {
    var userId = _userManager.GetUserId(User);
    var items = await _context.CartItems
      .Include(c => c.Product)
      .Where(c => c.ApplicationUserId == userId)
      .ToListAsync();

    if (!items.Any()) return RedirectToAction("Index", "Marketplace");

    float subtotal = items.Sum(x => x.Subtotal);
    float discount = subtotal > 50.0f ? subtotal * 0.10f : 0f;

    ViewBag.Subtotal = subtotal;
    ViewBag.Discount = discount;
    ViewBag.Total = subtotal - discount;

    return View(items);
  }

  [HttpPost]
  public async Task<IActionResult> PlaceOrder()
  {
    var userId = _userManager.GetUserId(User);
    var items = await _context.CartItems
      .Include(c => c.Product)
      .Where(c => c.ApplicationUserId == userId)
      .ToListAsync();

    if (!items.Any()) return RedirectToAction("Index", "Marketplace");

    var user = await _userManager.FindByIdAsync(userId!);
    if (user == null) return RedirectToAction("Login", "Auth");

    float subtotal = items.Sum(x => x.Subtotal);
    float discount = subtotal > 50.0f ? subtotal * 0.10f : 0f;
    float finalTotal = subtotal - discount;

    // Create the Order
    var order = new Order
    {
      UserId = user.Id,
      OrderDate = DateTime.UtcNow,
      TotalAmount = finalTotal,
      OrderItems = items.Select(i => new OrderItem
      {
        ShopItemId = i.ShopItemId,
        Quantity = i.Quantity,
        PriceAtPurchase = i.Product?.Price ?? 0,
        RecipeId = i.OriginatingRecipeId
      }).ToList()
    };

    _context.Orders.Add(order);
    
    // Clear the cart items from database
    _context.CartItems.RemoveRange(items);
    
    await _context.SaveChangesAsync();

    // Record revenue for each item with an originating recipe
    foreach (var orderItem in order.OrderItems)
    {
        if (orderItem.RecipeId.HasValue)
        {
            decimal itemTotal = (decimal)(orderItem.Quantity * orderItem.PriceAtPurchase);
            await _revenueService.RecordRevenueAsync(orderItem.RecipeId.Value, orderItem.Id, itemTotal);
        }
    }
    
    await _context.SaveChangesAsync();

    return View("Success", order.Id);
  }
}