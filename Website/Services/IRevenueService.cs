using Website.Models.Dtos;

namespace Website.Services;

public interface IRevenueService
{
    Task<bool> RecordRevenueAsync(int recipeId, int orderItemId, decimal amount);
    Task<decimal> GetAuthorTotalRevenueAsync(string userId);
    Task<List<RevenueTransactionDto>> GetAuthorTransactionsAsync(string userId);
}