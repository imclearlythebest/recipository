using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Website.Data;
using Website.Models;

namespace Website.Controllers;

public class CheckoutController : Controller
{
  private readonly AppDbContext _context;
  private readonly UserManager<ApplicationUser> _userManager;

  public CheckoutController(AppDbContext context, UserManager<ApplicationUser> userManager)
  {
    _context = context;
    _userManager = userManager;
  }

  public IActionResult Index()
  {
    var items = CartController.GetCartItems();
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
    var items = CartController.GetCartItems();
    if (!items.Any()) return RedirectToAction("Index", "Marketplace");

    var user = await _userManager.GetUserAsync(User);
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
        PriceAtPurchase = i.Product?.Price ?? 0
      }).ToList()
    };

    _context.Orders.Add(order);
    await _context.SaveChangesAsync();

    // Clear the cart after successful order
    CartController.ClearCart();

    return View("Success", order.Id);
  }
}