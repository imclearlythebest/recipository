using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Website.Services;
using System.Security.Claims;

namespace Website.Controllers;

[Authorize(Roles = "Admin")]
public class ApprovalController : Controller
{
    private readonly IApprovalService _approvalService;
    private readonly IRecipeService _recipeService;
    private readonly ILogger<ApprovalController> _logger;

    public ApprovalController(IApprovalService approvalService, IRecipeService recipeService, ILogger<ApprovalController> logger)
    {
        _approvalService = approvalService;
        _recipeService = recipeService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        var pending = await _approvalService.GetPendingRecipesAsync(page);
        ViewBag.TotalCount = await _approvalService.GetPendingRecipeCountAsync();
        ViewBag.CurrentPage = page;
        return View(pending);
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var recipe = await _recipeService.GetRecipeAsync(id);
            if (recipe.Status != "PendingApproval")
                return RedirectToAction(nameof(Index));

            var history = await _approvalService.GetApprovalHistoryAsync(id);
            ViewBag.History = history;
            return View(recipe);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId)) return Unauthorized();

        var success = await _approvalService.ApproveRecipeAsync(adminId, id);
        if (success)
            TempData["Success"] = "Recipe approved and published!";
        else
            TempData["Error"] = "Failed to approve recipe.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["Error"] = "Rejection reason is required.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminId)) return Unauthorized();

        var success = await _approvalService.RejectRecipeAsync(adminId, id, reason);
        if (success)
            TempData["Success"] = "Recipe rejected and returned to author.";
        else
            TempData["Error"] = "Failed to reject recipe.";

        return RedirectToAction(nameof(Index));
    }
}