namespace Website.Services;

public interface ICalorieService
{
    Task<float> CalculateRecipeCaloriesAsync(int recipeId);
    float CalculateIngredientCalories(float quantity, string unit, float caloriesPer100g);
    float ConvertUnitToGrams(float quantity, string unit);
}