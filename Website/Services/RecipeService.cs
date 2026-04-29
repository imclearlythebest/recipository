using Microsoft.EntityFrameworkCore;
using Website.Data;
using Website.Models;
using Website.Models.Dtos;

namespace Website.Services;

public class RecipeService : IRecipeService
{
    private readonly AppDbContext _context;
    private readonly ILogger<RecipeService> _logger;
    private readonly ICalorieService _calorieService;
    
    public RecipeService(AppDbContext context, ILogger<RecipeService> logger, ICalorieService calorieService)
    {
        _context = context;
        _logger = logger;
        _calorieService = calorieService;
    }
    
    public async Task<RecipeResponseDto> CreateRecipeAsync(string userId, CreateRecipeDto dto)
    {
        // Validate user exists
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new UnauthorizedAccessException("User not found");
        
        // Validate ingredients exist
        var ingredientIds = dto.Ingredients.Select(i => i.IngredientId).ToList();
        var ingredients = await _context.Ingredients
            .Where(i => ingredientIds.Contains(i.Id))
            .ToListAsync();
        
        if (ingredients.Count != dto.Ingredients.Count)
            throw new ArgumentException("One or more ingredients not found");
        
        // Create recipe in Draft status
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
        
        // Add ingredients
        foreach (var ingredientDto in dto.Ingredients)
        {
            var ingredient = ingredients.First(i => i.Id == ingredientDto.IngredientId);
            
            var recipeIngredient = new RecipeIngredient
            {
                RecipeId = recipe.Id,
                IngredientId = ingredient.Id,
                Quantity = ingredientDto.Quantity,
                Unit = ingredientDto.Unit,
                CaloriesPerUnit = ingredient.Calories / 100f // assumes calories per 100g
            };
            
            _context.RecipeIngredients.Add(recipeIngredient);
        }
        
        await _context.SaveChangesAsync();
        
        // Calculate total calories
        await _calorieService.CalculateRecipeCaloriesAsync(recipe.Id);
        
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
            .Where(r => r.ApplicationUserId == userId && (r.Status == "Draft" || r.Status == "Rejected"))
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
        return await _calorieService.CalculateRecipeCaloriesAsync(recipeId);
    }

    public async Task<RecipeResponseDto> UpdateRecipeAsync(string userId, UpdateRecipeDto dto)
    {
        if (!await CanUserEditRecipeAsync(userId, dto.Id))
            throw new UnauthorizedAccessException("You do not have permission to edit this recipe or it is not in an editable state.");

        var recipe = await _context.Recipes
            .Include(r => r.RecipeIngredients)
            .FirstOrDefaultAsync(r => r.Id == dto.Id);

        if (recipe == null)
            throw new ArgumentException("Recipe not found");

        // Update fields
        recipe.Title = dto.Title;
        recipe.MainText = dto.MainText;
        recipe.Instructions = dto.Instructions;
        recipe.PrepTime = dto.PrepTime;
        recipe.CookTime = dto.CookTime;
        recipe.ServingSize = dto.ServingSize;
        recipe.Difficulty = dto.Difficulty;

        // Reset status if Rejected
        if (recipe.Status == "Rejected")
            recipe.Status = "Draft";

        // Replace ingredients
        _context.RecipeIngredients.RemoveRange(recipe.RecipeIngredients);

        var ingredientIds = dto.Ingredients.Select(i => i.IngredientId).ToList();
        var ingredients = await _context.Ingredients
            .Where(i => ingredientIds.Contains(i.Id))
            .ToListAsync();

        if (ingredients.Count != dto.Ingredients.Count)
            throw new ArgumentException("One or more ingredients not found");

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

        // Recalculate calories
        await _calorieService.CalculateRecipeCaloriesAsync(recipe.Id);

        _logger.LogInformation($"Recipe {recipe.Id} updated by user {userId}");

        return await GetRecipeAsync(recipe.Id);
    }

    public async Task<bool> DeleteRecipeAsync(string userId, int recipeId)
    {
        if (!await CanUserDeleteRecipeAsync(userId, recipeId))
            throw new UnauthorizedAccessException("You do not have permission to delete this recipe or it cannot be deleted.");

        var recipe = await _context.Recipes
            .Include(r => r.RecipeIngredients)
            .Include(r => r.ApprovalHistory)
            .FirstOrDefaultAsync(r => r.Id == recipeId);

        if (recipe == null)
            return false;

        // Delete associated revenue (pending only as per rules)
        var pendingRevenue = await _context.RecipeRevenues
            .Where(rr => rr.RecipeId == recipeId && rr.Status == "Pending")
            .ToListAsync();
        _context.RecipeRevenues.RemoveRange(pendingRevenue);

        _context.RecipeIngredients.RemoveRange(recipe.RecipeIngredients);
        _context.RecipeApprovals.RemoveRange(recipe.ApprovalHistory);
        _context.Recipes.Remove(recipe);

        await _context.SaveChangesAsync();
        _logger.LogInformation($"Recipe {recipeId} deleted by user {userId}");

        return true;
    }

    public async Task<bool> CanUserEditRecipeAsync(string userId, int recipeId)
    {
        var recipe = await _context.Recipes.FindAsync(recipeId);
        if (recipe == null) return false;

        return recipe.ApplicationUserId == userId && (recipe.Status == "Draft" || recipe.Status == "Rejected");
    }

    public async Task<bool> CanUserDeleteRecipeAsync(string userId, int recipeId)
    {
        var recipe = await _context.Recipes.FindAsync(recipeId);
        if (recipe == null) return false;

        if (recipe.ApplicationUserId != userId) return false;
        if (recipe.Status != "Draft" && recipe.Status != "Rejected") return false;

        // Check for completed revenue
        var hasCompletedRevenue = await _context.RecipeRevenues
            .AnyAsync(rr => rr.RecipeId == recipeId && rr.Status == "Completed");

        return !hasCompletedRevenue;
    }

    public async Task<bool> SubmitForApprovalAsync(string userId, int recipeId)
    {
        var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Id == recipeId && r.ApplicationUserId == userId);
        
        if (recipe == null) return false;
        if (recipe.Status != "Draft" && recipe.Status != "Rejected") return false;

        recipe.Status = "PendingApproval";
        await _context.SaveChangesAsync();
        
        _logger.LogInformation($"Recipe {recipeId} submitted for approval by user {userId}");
        return true;
    }
    
    private RecipeResponseDto MapToDto(Recipe recipe)
    {
        var ingredients = recipe.RecipeIngredients.Select(ri => new RecipeIngredientDto
        {
            Id = ri.Id,
            IngredientId = ri.IngredientId,
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
