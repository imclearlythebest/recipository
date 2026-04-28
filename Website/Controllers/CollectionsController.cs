using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website.Data;
using Website.Models;
using Website.Models.ViewModels;

namespace Website.Controllers;

public class CollectionsController(AppDbContext dbContext, UserManager<ApplicationUser> userManager) : Controller
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var collections = await _dbContext.Collections
            .Where(c => c.ApplicationUserId == currentUser.Id)
            .Include(c => c.Recipes)
            .OrderBy(c => c.Name)
            .ToListAsync();

        var viewModel = new CollectionsIndexViewModel
        {
            OwnedCollections = collections
        };

        return View(viewModel);
    }

    [Authorize]
    [HttpGet]
    public IActionResult Create() => View(new CollectionCreateViewModel());

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CollectionCreateViewModel model)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(nameof(CollectionCreateViewModel.Name), "Collection name is required.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var collection = new Collection
        {
            ApplicationUserId = currentUser.Id,
            Name = model.Name,
            CoverPhotoUrl = model.CoverPhotoUrl,
            PublicUrl = Guid.NewGuid().ToString("N")
        };

        _dbContext.Collections.Add(collection);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = collection.Id });
    }

    [Authorize]
    public async Task<IActionResult> Details(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var collection = await _dbContext.Collections
            .Include(c => c.User)
            .Include(c => c.Recipes)
            .ThenInclude(r => r.Author)
            .FirstOrDefaultAsync(c => c.Id == id && c.ApplicationUserId == currentUser.Id);

        if (collection == null) return NotFound();

        ViewData["PublicShareUrl"] = Url.Action(nameof(Public), "Collections", new { publicUrl = collection.PublicUrl }, Request.Scheme);
        return View(collection);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var collection = await _dbContext.Collections
            .FirstOrDefaultAsync(c => c.Id == id && c.ApplicationUserId == currentUser.Id);

        if (collection == null) return NotFound();

        var vm = new CollectionCreateViewModel
        {
            Name = collection.Name,
            CoverPhotoUrl = collection.CoverPhotoUrl
        };

        ViewData["CollectionId"] = id;
        return View(vm);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CollectionCreateViewModel model)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var collection = await _dbContext.Collections
            .FirstOrDefaultAsync(c => c.Id == id && c.ApplicationUserId == currentUser.Id);

        if (collection == null) return NotFound();

        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(nameof(CollectionCreateViewModel.Name), "Collection name is required.");
        }

        if (!ModelState.IsValid)
        {
            ViewData["CollectionId"] = id;
            return View(model);
        }

        collection.Name = model.Name;
        collection.CoverPhotoUrl = model.CoverPhotoUrl;

        _dbContext.Collections.Update(collection);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = id });
    }

    [AllowAnonymous]
    public async Task<IActionResult> Public(string publicUrl)
    {
        var collection = await _dbContext.Collections
            .Include(c => c.User)
            .Include(c => c.Recipes)
            .ThenInclude(r => r.Author)
            .FirstOrDefaultAsync(c => c.PublicUrl == publicUrl);

        if (collection == null) return NotFound();

        ViewData["IsPublicView"] = true;
        ViewData["PublicShareUrl"] = Url.Action(nameof(Public), "Collections", new { publicUrl }, Request.Scheme);
        return View("Details", collection);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRecipe(int collectionId, int recipeId, string? returnUrl = null)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var collection = await _dbContext.Collections
            .Include(c => c.Recipes)
            .FirstOrDefaultAsync(c => c.Id == collectionId && c.ApplicationUserId == currentUser.Id);

        if (collection == null) return NotFound();

        var recipe = await _dbContext.Recipes.FindAsync(recipeId);
        if (recipe == null) return NotFound();

        if (!collection.Recipes.Any(r => r.Id == recipeId))
        {
            collection.Recipes.Add(recipe);
            await _dbContext.SaveChangesAsync();
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(Details), new { id = collectionId });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveRecipe(int collectionId, int recipeId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var collection = await _dbContext.Collections
            .Include(c => c.Recipes)
            .FirstOrDefaultAsync(c => c.Id == collectionId && c.ApplicationUserId == currentUser.Id);

        if (collection == null) return NotFound();

        var recipe = collection.Recipes.FirstOrDefault(r => r.Id == recipeId);
        if (recipe != null)
        {
            collection.Recipes.Remove(recipe);
            await _dbContext.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { id = collectionId });
    }
}