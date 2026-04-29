using Microsoft.EntityFrameworkCore;
using Website.Data;
using Website.Models;
using Website.Models.Dtos;

namespace Website.Services;

public class RecipeService : IRecipeService
{
    private readonly AppDbContext _context;
    private readonly ILogger<RecipeService> _logger;
    
    public RecipeService(AppDbContext context, ILogger<RecipeService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<RecipeResponseDto> CreateRecipeAsync(string userId, CreateRecipeDto dto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new UnauthorizedAccessException("User not found");
        
        var ingredientIds = dto.Ingredients.Select(i => i.IngredientId).ToList();
        var ingredients = await _context.Ingredients
            .Where(i => ingredientIds.Contains(i.Id))
            .ToListAsync();
        
        if (ingredients.Count != dto.Ingredients.Count)
            throw new ArgumentException("One or more ingredients not found");
        
        var recipe = new Recipe
        {
            Title = dto.Title,
            MainText = dto.MainText,
            Instructions = dto.Instructions,
            PrepTime = dto.PrepTime,
            CookTime = dto.CookTime,
            ServingSize = dto.ServingSize,
            Difficulty = dto.Difficulty,
            Status = "Draft",
            EstimatedCalories = 0,
            CreatedAt = DateTime.UtcNow,
            ApplicationUserId = userId,
            Author = user
        };
        
        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync();
        
        foreach (var ingredientDto in dto.Ingredients)
        {
            var ingredient = ingredients.First(i => i.Id == ingredientDto.IngredientId);
            var recipeIngredient = new RecipeIngredient
            {
                RecipeId = recipe.Id,
                IngredientId = ingredient.Id,
                Quantity = ingredientDto.Quantity,
                Unit = ingredientDto.Unit,
                CaloriesPerUnit = ingredient.Calories / 100f
            };
            
            _context.RecipeIngredients.Add(recipeIngredient);
        }
        
        await _context.SaveChangesAsync();
        await CalculateRecipeCaloriesAsync(recipe.Id);
        
        _logger.LogInformation($"Recipe {recipe.Id} created by user {userId} in Draft status");
        
        return await GetRecipeAsync(recipe.Id);
    }
    
    public async Task<RecipeResponseDto> GetRecipeAsync(int id)
    {
        var recipe = await _context.Recipes
            .Include(r => r.Author)
            .Include(r => r.RecipeIngredients)
            .ThenInclude(ri => ri.Ingredient)
            .FirstOrDefaultAsync(r => r.Id == id);
        
        if (recipe == null)
            throw new ArgumentException($"Recipe {id} not found");
        
        return MapToDto(recipe);
    }

    public async Task<List<RecipeResponseDto>> GetUserRecipesAsync(string userId)
    {
        var recipes = await _context.Recipes
            .Where(r => r.ApplicationUserId == userId)
            .Include(r => r.Author)
            .Include(r => r.RecipeIngredients)
            .ThenInclude(ri => ri.Ingredient)
            .ToListAsync();
        
        return recipes.Select(r => MapToDto(r)).ToList();
    }
    
    public async Task<List<RecipeResponseDto>> GetUserDraftRecipesAsync(string userId)
    {
        var recipes = await _context.Recipes
            .Where(r => r.ApplicationUserId == userId && r.Status == "Draft")
            .Include(r => r.Author)
            .Include(r => r.RecipeIngredients)
            .ThenInclude(ri => ri.Ingredient)
            .ToListAsync();
        
        return recipes.Select(r => MapToDto(r)).ToList();
    }
    
    public async Task<List<RecipeResponseDto>> GetApprovedRecipesAsync(int pageNumber = 1, int pageSize = 12)
    {
        var recipes = await _context.Recipes
            .Where(r => r.Status == "Published")
            .Include(r => r.Author)
            .Include(r => r.RecipeIngredients)
            .ThenInclude(ri => ri.Ingredient)
            .OrderByDescending(r => r.PublishedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return recipes.Select(r => MapToDto(r)).ToList();
    }

    public async Task<List<RecipeResponseDto>> SearchRecipesAsync(string searchTerm)
    {
        var recipes = await _context.Recipes
            .Where(r => r.Status == "Published" && (r.Title.Contains(searchTerm) || r.MainText.Contains(searchTerm)))
            .Include(r => r.Author)
            .Include(r => r.RecipeIngredients)
            .ThenInclude(ri => ri.Ingredient)
            .ToListAsync();
        
        return recipes.Select(r => MapToDto(r)).ToList();
    }
    
    public async Task<float> CalculateRecipeCaloriesAsync(int recipeId)
    {
        var ingredients = await _context.RecipeIngredients
            .Where(ri => ri.RecipeId == recipeId)
            .ToListAsync();
        
        float totalCalories = 0;
        foreach (var ingredient in ingredients)
        {
            totalCalories += ingredient.TotalCalories;
        }
        
        var recipe = await _context.Recipes.FindAsync(recipeId);
        if (recipe != null)
        {
            recipe.EstimatedCalories = totalCalories;
            await _context.SaveChangesAsync();
        }
        
        return totalCalories;
    }
    
    private RecipeResponseDto MapToDto(Recipe recipe)
    {
        var ingredients = recipe.RecipeIngredients.Select(ri => new RecipeIngredientDto
        {
            Id = ri.Id,
            IngredientName = ri.Ingredient.Name ?? "Unknown",
            Quantity = ri.Quantity,
            Unit = ri.Unit,
            TotalCalories = ri.TotalCalories
        }).ToList();
        
        return new RecipeResponseDto
        {
            Id = recipe.Id,
            Title = recipe.Title,
            MainText = recipe.MainText,
            Instructions = recipe.Instructions,
            PrepTime = recipe.PrepTime,
            CookTime = recipe.CookTime,
            ServingSize = recipe.ServingSize,
            Difficulty = recipe.Difficulty,
            Status = recipe.Status,
            EstimatedCalories = recipe.EstimatedCalories,
            CreatedAt = recipe.CreatedAt,
            PublishedAt = recipe.PublishedAt,
            AuthorId = recipe.ApplicationUserId,
            AuthorName = recipe.Author?.DisplayName ?? recipe.Author?.UserName ?? "Anonymous",
            Ingredients = ingredients
        };
    }
}
