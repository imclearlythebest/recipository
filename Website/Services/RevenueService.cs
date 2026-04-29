using Microsoft.EntityFrameworkCore;
using Website.Data;
using Website.Models;
using Website.Models.Dtos;

namespace Website.Services;

public class RevenueService : IRevenueService
{
    private readonly AppDbContext _context;
    private readonly ILogger<RevenueService> _logger;

    public RevenueService(AppDbContext context, ILogger<RevenueService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> RecordRevenueAsync(int recipeId, int orderItemId, decimal amount)
    {
        try
        {
            var recipe = await _context.Recipes.FindAsync(recipeId);
            if (recipe == null)
            {
                _logger.LogWarning($"Attempted to record revenue for non-existent recipe {recipeId}");
                return false;
            }

            var authorShare = amount * 0.05m;
            var platformShare = amount * 0.95m;

            var revenue = new RecipeRevenue
            {
                RecipeId = recipeId,
                OrderItemId = orderItemId,
                Amount = amount,
                AuthorShare = authorShare,
                PlatformShare = platformShare,
                TransactionDate = DateTime.UtcNow,
                Status = "Completed" // Default to completed for this simplified implementation
            };

            _context.RecipeRevenues.Add(revenue);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Recorded revenue for recipe {recipeId}: Author Share = {authorShare}, Platform Share = {platformShare}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error recording revenue for recipe {recipeId}");
            return false;
        }
    }

    public async Task<decimal> GetAuthorTotalRevenueAsync(string userId)
    {
        return await _context.RecipeRevenues
            .Where(rr => rr.Recipe.ApplicationUserId == userId && rr.Status == "Completed")
            .SumAsync(rr => rr.AuthorShare);
    }

    public async Task<List<RevenueTransactionDto>> GetAuthorTransactionsAsync(string userId)
    {
        return await _context.RecipeRevenues
            .Where(rr => rr.Recipe.ApplicationUserId == userId)
            .OrderByDescending(rr => rr.TransactionDate)
            .Select(rr => new RevenueTransactionDto
            {
                Id = rr.Id,
                RecipeTitle = rr.Recipe.Title,
                TotalAmount = rr.Amount,
                AuthorShare = rr.AuthorShare,
                Status = rr.Status,
                TransactionDate = rr.TransactionDate
            })
            .ToListAsync();
    }
}