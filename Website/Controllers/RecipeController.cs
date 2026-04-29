using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website.Data;
using Website.Models;
using Website.Models.Dtos;
using Website.Services;
using System.Security.Claims;

namespace Website.Controllers;

[Authorize]
public class RecipeController : Controller
{
    private readonly IRecipeService _recipeService;
    private readonly AppDbContext _context;
    private readonly ILogger<RecipeController> _logger;

    public RecipeController(IRecipeService recipeService, AppDbContext context, ILogger<RecipeController> logger)
    {
        _recipeService = recipeService;
        _context = context;
        _logger = logger;
    }

    // GET: Display create recipe form
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        try
        {
            var ingredients = await _context.Ingredients.ToListAsync();
            return View(ingredients);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading create recipe page: {ex.Message}");
            TempData["Error"] = "Error loading recipe creation page";
            return RedirectToAction("Index", "Home");
        }
    }

    // POST: Create recipe
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRecipeDto dto)
    {
        if (!ModelState.IsValid)
        {
            var ingredients = await _context.Ingredients.ToListAsync();
            return View(ingredients);
        }

        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Auth");

            var result = await _recipeService.CreateRecipeAsync(userId, dto);

            TempData["Success"] = "Recipe created successfully as Draft! You can now edit and submit for approval.";
            return RedirectToAction("Index", "Home");
        }
        catch (UnauthorizedAccessException)
        {
            TempData["Error"] = "You must be logged in to create a recipe";
            return RedirectToAction("Login", "Auth");
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError("", ex.Message);
            var ingredients = await _context.Ingredients.ToListAsync();
            return View(ingredients);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating recipe: {ex.Message}");
            ModelState.AddModelError("", "An error occurred while creating the recipe");
            var ingredients = await _context.Ingredients.ToListAsync();
            return View(ingredients);
        }
    }

    // GET: View recipe details
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var recipe = await _recipeService.GetRecipeAsync(id);

            // Authorization check: only show draft recipes to author
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (recipe.Status == "Draft" && recipe.AuthorId != userId)
                return NotFound();

            var ingredientIds = recipe.Ingredients.Select(i => i.IngredientId).ToList();
            var shopItems = await _context.Ingredients.OfType<ShopItem>()
                .Where(i => ingredientIds.Contains(i.Id))
                .ToDictionaryAsync(i => i.Id, i => i.Price);
            
            ViewBag.ShopItems = shopItems;

            return View(recipe);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching recipe {id}: {ex.Message}");
            TempData["Error"] = "Error loading recipe";
            return RedirectToAction("Index", "Home");
        }
    }

    // GET: List user's draft recipes
    public async Task<IActionResult> MyDrafts()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Auth");

            var drafts = await _recipeService.GetUserDraftRecipesAsync(userId);
            return View(drafts);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading drafts: {ex.Message}");
            TempData["Error"] = "Error loading your drafts";
            return RedirectToAction("Index", "Home");
        }
    }

    // GET: Display edit recipe form
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Auth");

            if (!await _recipeService.CanUserEditRecipeAsync(userId, id))
            {
                TempData["Error"] = "You do not have permission to edit this recipe or it is not in an editable state.";
                return RedirectToAction("MyDrafts");
            }

            var recipe = await _recipeService.GetRecipeAsync(id);
            var ingredients = await _context.Ingredients.ToListAsync();
            
            ViewBag.Ingredients = ingredients;
            return View(recipe);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading edit recipe page: {ex.Message}");
            TempData["Error"] = "Error loading edit page";
            return RedirectToAction("MyDrafts");
        }
    }

    // POST: Update recipe
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateRecipeDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Ingredients = await _context.Ingredients.ToListAsync();
            var recipe = await _recipeService.GetRecipeAsync(dto.Id);
            return View(recipe);
        }

        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Auth");

            await _recipeService.UpdateRecipeAsync(userId, dto);

            TempData["Success"] = "Recipe updated successfully!";
            return RedirectToAction("Details", new { id = dto.Id });
        }
        catch (UnauthorizedAccessException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("MyDrafts");
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError("", ex.Message);
            ViewBag.Ingredients = await _context.Ingredients.ToListAsync();
            var recipe = await _recipeService.GetRecipeAsync(dto.Id);
            return View(recipe);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating recipe: {ex.Message}");
            ModelState.AddModelError("", "An error occurred while updating the recipe");
            ViewBag.Ingredients = await _context.Ingredients.ToListAsync();
            var recipe = await _recipeService.GetRecipeAsync(dto.Id);
            return View(recipe);
        }
    }

    // POST: Delete recipe
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Auth");

            var success = await _recipeService.DeleteRecipeAsync(userId, id);

            if (success)
                TempData["Success"] = "Recipe deleted successfully";
            else
                TempData["Error"] = "Could not delete recipe";

            return RedirectToAction("MyDrafts");
        }
        catch (UnauthorizedAccessException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("MyDrafts");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting recipe: {ex.Message}");
            TempData["Error"] = "An error occurred while deleting the recipe";
            return RedirectToAction("MyDrafts");
        }
    }

    // POST: Submit for approval
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Auth");

            var success = await _recipeService.SubmitForApprovalAsync(userId, id);

            if (success)
                TempData["Success"] = "Recipe submitted for approval successfully!";
            else
                TempData["Error"] = "Could not submit recipe for approval. Ensure it's in Draft or Rejected status.";

            return RedirectToAction("MyDrafts");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error submitting recipe: {ex.Message}");
            TempData["Error"] = "An error occurred while submitting the recipe";
            return RedirectToAction("MyDrafts");
        }
    }
}
