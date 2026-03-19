using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website.Data;
using Website.Models;

namespace Website.Controllers;

public class MarketplaceController : Controller
{
  private readonly AppDbContext _context;

  public MarketplaceController(AppDbContext context) => _context = context;

  public async Task<IActionResult> Index()
  {
    // OfType<ShopItem> filters the Ingredients table for only ShopItems
    var items = await _context.Ingredients.OfType<ShopItem>().ToListAsync();
    return View(items);
  }
}