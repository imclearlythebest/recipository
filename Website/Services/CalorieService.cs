using Microsoft.EntityFrameworkCore;
using Website.Data;

namespace Website.Services;

public class CalorieService : ICalorieService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CalorieService> _logger;

    public CalorieService(AppDbContext context, ILogger<CalorieService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<float> CalculateRecipeCaloriesAsync(int recipeId)
    {
        var recipe = await _context.Recipes
            .Include(r => r.RecipeIngredients)
            .FirstOrDefaultAsync(r => r.Id == recipeId);

        if (recipe == null) return 0;

        float totalCalories = 0;
        foreach (var ri in recipe.RecipeIngredients)
        {
            totalCalories += CalculateIngredientCalories(ri.Quantity, ri.Unit, ri.CaloriesPerUnit * 100); // CaloriesPerUnit in RecipeIngredient was stored as calories per 1g in RecipeService.cs
        }

        recipe.EstimatedCalories = totalCalories;
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Recalculated calories for recipe {recipeId}: {totalCalories} cal");
        return totalCalories;
    }

    public float CalculateIngredientCalories(float quantity, string unit, float caloriesPer100g)
    {
        float grams = ConvertUnitToGrams(quantity, unit);
        return (grams / 100f) * caloriesPer100g;
    }

    public float ConvertUnitToGrams(float quantity, string unit)
    {
        if (string.IsNullOrWhiteSpace(unit)) return quantity;

        unit = unit.ToLower().Trim();

        return unit switch
        {
            "g" or "gram" or "grams" => quantity,
            "kg" or "kilogram" or "kilograms" => quantity * 1000f,
            "ml" or "millilitre" or "millilitres" => quantity, // Assuming 1ml = 1g for common liquids
            "l" or "litre" or "litres" => quantity * 1000f,
            "cup" or "cups" => quantity * 240f,
            "tbsp" or "tablespoon" or "tablespoons" => quantity * 15f,
            "tsp" or "teaspoon" or "teaspoons" => quantity * 5f,
            "oz" or "ounce" or "ounces" => quantity * 28.35f,
            "lb" or "pound" or "pounds" => quantity * 453.59f,
            _ => quantity // Default to 1:1 if unit is unknown
        };
    }
}