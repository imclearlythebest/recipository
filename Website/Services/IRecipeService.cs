using Website.Models.Dtos;

namespace Website.Services;

public interface IRecipeService
{
    Task<RecipeResponseDto> CreateRecipeAsync(string userId, CreateRecipeDto dto);
    Task<RecipeResponseDto> GetRecipeAsync(int id);
    Task<List<RecipeResponseDto>> GetUserRecipesAsync(string userId);
    Task<List<RecipeResponseDto>> GetUserDraftRecipesAsync(string userId);
    Task<List<RecipeResponseDto>> GetApprovedRecipesAsync(int pageNumber = 1, int pageSize = 12);
    Task<List<RecipeResponseDto>> SearchRecipesAsync(string searchTerm);
    Task<float> CalculateRecipeCaloriesAsync(int recipeId);
    Task<RecipeResponseDto> UpdateRecipeAsync(string userId, UpdateRecipeDto dto);
    Task<bool> DeleteRecipeAsync(string userId, int recipeId);
    Task<bool> CanUserEditRecipeAsync(string userId, int recipeId);
    Task<bool> CanUserDeleteRecipeAsync(string userId, int recipeId);
    Task<bool> SubmitForApprovalAsync(string userId, int recipeId);
}
