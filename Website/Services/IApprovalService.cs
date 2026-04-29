using Website.Models.Dtos;

namespace Website.Services;

public interface IApprovalService
{
    Task<bool> ApproveRecipeAsync(string adminUserId, int recipeId);
    Task<bool> RejectRecipeAsync(string adminUserId, int recipeId, string reason);
    Task<List<PendingRecipeDto>> GetPendingRecipesAsync(int pageNumber = 1, int pageSize = 20);
    Task<int> GetPendingRecipeCountAsync();
    Task<List<ApprovalHistoryDto>> GetApprovalHistoryAsync(int recipeId);
}