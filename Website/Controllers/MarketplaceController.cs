using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website.Data;

namespace Website.Controllers;

public class MarketplaceController : Controller
{
  private readonly AppDbContext _context;

  public MarketplaceController(AppDbContext context) => _context = context;

  public async Task<IActionResult> Index()
  {
    // Change .Ingredients to .Contents
  var items = await _context.Ingredients.ToListAsync(); 
  return View(items);
  }
}