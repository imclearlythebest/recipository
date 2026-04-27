using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website.Data;
using Website.Models;


namespace Website.Controllers;

public class IngredientsController(AppDbContext dbContext) : Controller
{
    private readonly AppDbContext _dbContext = dbContext;

    private void SetIngredientFormMeta(string title, string subtitle, string submitLabel, string formAction, bool isEdit)
    {
        ViewData["FormTitle"] = title;
        ViewData["FormSubtitle"] = subtitle;
        ViewData["SubmitLabel"] = submitLabel;
        ViewData["FormAction"] = formAction;
        ViewData["IsEdit"] = isEdit;
    }

    private void SetShopItemFormMeta(string title, string subtitle, string submitLabel, string formAction, bool isEdit, int? originalId = null)
    {
        ViewData["FormTitle"] = title;
        ViewData["FormSubtitle"] = subtitle;
        ViewData["SubmitLabel"] = submitLabel;
        ViewData["FormAction"] = formAction;
        ViewData["IsEdit"] = isEdit;
        ViewData["OriginalId"] = originalId;
    }

    public IActionResult Index()
    {
        var ingredients = _dbContext.Ingredients.ToList();
        return View(ingredients);
    }

    public async Task<IActionResult> Filter(string? search, IngredientType? type)
    {
        var query = _dbContext.Ingredients.AsQueryable();

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

        var ingredients = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(query);
        return PartialView("_IngredientsGrid", ingredients);
    }

    [HttpGet]
    public IActionResult Create()
    {
        SetIngredientFormMeta("Create New Ingredient", "Define a base ingredient for recipes", "Create Ingredient", "Create", false);
        return View("IngredientForm", new Ingredient());
    }

    [HttpGet]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var ingredient = await _dbContext.Ingredients.FindAsync(id);
        if (ingredient == null) return NotFound();

        if (ingredient is ShopItem shopItem)
        {
            SetShopItemFormMeta("Edit Shop Item", "Update this marketplace item", "Save Changes", "EditShopItem", true);
            return View("ShopItemForm", shopItem);
        }

        SetIngredientFormMeta("Edit Ingredient", "Update this base ingredient", "Save Changes", "Edit", true);
        return View("IngredientForm", ingredient);
    }

    [HttpPost]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Ingredient ingredient)
    {
        if (id != ingredient.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            SetIngredientFormMeta("Edit Ingredient", "Update this base ingredient", "Save Changes", "Edit", true);
            return View("IngredientForm", ingredient);
        }

        var existing = await _dbContext.Ingredients.FindAsync(id);
        if (existing == null || existing is ShopItem) return NotFound();

        existing.Name = ingredient.Name;
        existing.Description = ingredient.Description;
        existing.ImageUrl = ingredient.ImageUrl;
        existing.Calories = ingredient.Calories;
        existing.Type = ingredient.Type;

        await _dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditShopItem(int id, ShopItem shopItem)
    {
        if (id != shopItem.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            SetShopItemFormMeta("Edit Shop Item", "Update this marketplace item", "Save Changes", "EditShopItem", true);
            return View("ShopItemForm", shopItem);
        }

        var existing = await _dbContext.Ingredients.OfType<ShopItem>()
            .FirstOrDefaultAsync(i => i.Id == id);

        if (existing == null) return NotFound();

        existing.Name = shopItem.Name;
        existing.Description = shopItem.Description;
        existing.ImageUrl = shopItem.ImageUrl;
        existing.Calories = shopItem.Calories;
        existing.Type = shopItem.Type;
        existing.Price = shopItem.Price;
        existing.Stock = shopItem.Stock;
        existing.Moq = shopItem.Moq;
        existing.Increment = shopItem.Increment;

        await _dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Create(Ingredient ingredient)
    {
        if (ModelState.IsValid)
        {
            _dbContext.Add(ingredient);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        SetIngredientFormMeta("Create New Ingredient", "Define a base ingredient for recipes", "Create Ingredient", "Create", false);
        return View("IngredientForm", ingredient);
    }

    [HttpGet]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> Convert(int id)
    {
        var ingredient = await _dbContext.Ingredients.FindAsync(id);
        if (ingredient == null) return NotFound();

        var shopItem = new ShopItem
        {
            Name = ingredient.Name,
            Description = ingredient.Description,
            ImageUrl = ingredient.ImageUrl,
            Calories = ingredient.Calories,
            Type = ingredient.Type
        };

        SetShopItemFormMeta("Convert Ingredient", "Convert this ingredient into a marketplace item", "Convert to Shop Item", "CreateShopItem", false, id);
        return View("ShopItemForm", shopItem);
    }

    [HttpGet]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public IActionResult CreateShopItem()
    {
        SetShopItemFormMeta("Create New Shop Item", "Add a new item to the global marketplace (Admin Only)", "Create Shop Item", "CreateShopItem", false);
        return View("ShopItemForm", new ShopItem());
    }

    [HttpPost]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateShopItem(ShopItem shopItem, int? originalId)
    {
        if (ModelState.IsValid)
        {
            if (originalId.HasValue)
            {
                var original = await _dbContext.Ingredients.FindAsync(originalId.Value);
                if (original != null)
                {
                    _dbContext.Ingredients.Remove(original);
                }
            }

            _dbContext.Add(shopItem);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("Index", "Marketplace");
        }
        if (originalId.HasValue)
        {
            SetShopItemFormMeta("Convert Ingredient", "Convert this ingredient into a marketplace item", "Convert to Shop Item", "CreateShopItem", false, originalId);
        }
        else
        {
            SetShopItemFormMeta("Create New Shop Item", "Add a new item to the global marketplace (Admin Only)", "Create Shop Item", "CreateShopItem", false);
        }
        return View("ShopItemForm", shopItem);
    }
}
