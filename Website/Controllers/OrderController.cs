using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Website.Data;
using Website.Models;
using Microsoft.AspNetCore.Authorization;

namespace Website.Controllers;

[Authorize]
public class OrderController : Controller
{
  private readonly AppDbContext _context;
  private readonly UserManager<ApplicationUser> _userManager;

  public OrderController(AppDbContext context, UserManager<ApplicationUser> userManager)
  {
    _context = context;
    _userManager = userManager;
  }

  // GET: /Order
  public async Task<IActionResult> Index()
  {
    var user = await _userManager.GetUserAsync(User);
    if (user == null) return Challenge();

    // Start with a query that includes items and the user who placed the order
    IQueryable<Order> query = _context.Orders
      .Include(o => o.OrderItems)
        .ThenInclude(oi => oi.Product)
      .Include(o => o.User);

    // Logic for Feature 4: 
    // If not Admin, filter so they only see their own orders.
    // If Admin, they see everything by default.
    if (!User.IsInRole("Admin"))
    {
      query = query.Where(o => o.UserId == user.Id);
    }

    var orders = await query
      .OrderByDescending(o => o.OrderDate)
      .ToListAsync();

    return View(orders);
  }

  // POST: /Order/UpdateStatus
  // Strictly restricted to Admin role to prevent users from self-delivering orders.
  [HttpPost]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> UpdateStatus(int orderId, string status)
  {
    var order = await _context.Orders
        .Include(o => o.OrderItems)
        .FirstOrDefaultAsync(o => o.Id == orderId);
    
    if (order != null)
    {
      string oldStatus = order.Status;
      order.Status = status;

      // If transition to Delivered, create review requests
      if (oldStatus != "Delivered" && status == "Delivered")
      {
          var uniqueRecipeIds = order.OrderItems
              .Where(oi => oi.RecipeId.HasValue)
              .Select(oi => oi.RecipeId!.Value)
              .Distinct();

          foreach (var recipeId in uniqueRecipeIds)
          {
              var alreadyHasRequest = await _context.RecipeReviewRequests
                  .AnyAsync(r => r.ApplicationUserId == order.UserId && r.RecipeId == recipeId);
              
              if (!alreadyHasRequest)
              {
                  _context.RecipeReviewRequests.Add(new RecipeReviewRequest
                  {
                      ApplicationUserId = order.UserId,
                      RecipeId = recipeId
                  });
              }
          }
      }

      await _context.SaveChangesAsync();
    }

    // Redirect back to the list to see the updated status
    return RedirectToAction(nameof(Index));
  }
}