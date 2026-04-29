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

  public async Task<IActionResult> Filter(string? search, IngredientType? type, bool hideOutOfStock, string? priceRange)
  {
    var query = _context.Ingredients.OfType<ShopItem>().AsQueryable();

    if (!string.IsNullOrEmpty(search))
    {
      var lowSearch = search.ToLower();
      query = query.Where(i => (i.Name != null && i.Name.ToLower().Contains(lowSearch)) || 
                               (i.Description != null && i.Description.ToLower().Contains(lowSearch)));
    }

    if (type.HasValue)
    {
      query = query.Where(i => i.Type == type.Value);
    }

    if (hideOutOfStock)
    {
      query = query.Where(i => i.Stock > 0);
    }

    if (!string.IsNullOrEmpty(priceRange) && priceRange != "all")
    {
      query = priceRange switch
      {
        "under-100" => query.Where(i => i.Price < 100),
        "100-500" => query.Where(i => i.Price >= 100 && i.Price <= 500),
        "over-500" => query.Where(i => i.Price > 500),
        _ => query
      };
    }

    var items = await query.ToListAsync();
    return PartialView("_MarketplaceGrid", items);
  }
}