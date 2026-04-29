using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website.Data;
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

            if (TempData != null)
                TempData["Success"] = "Recipe created successfully as Draft! You can now edit and submit for approval.";

            return RedirectToAction("Index", "Home");
        }
        catch (UnauthorizedAccessException)
        {
            if (TempData != null)
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

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var recipe = await _recipeService.GetRecipeAsync(id);
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (recipe.Status == "Draft" && recipe.AuthorId != userId)
                return NotFound();

            return View(recipe);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching recipe {id}: {ex.Message}");
            if (TempData != null)
                TempData["Error"] = "Error loading recipe";
            return RedirectToAction("Index", "Home");
        }
    }

    public async Task<IActionResult> MyDrafts()
    {
        try
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Auth");

            var drafts = await _recipeService.GetUserDraftRecipesAsync(userId);
            return View(drafts);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading drafts: {ex.Message}");
            if (TempData != null)
                TempData["Error"] = "Error loading your drafts";
            return RedirectToAction("Index", "Home");
        }
    }
}
