using Microsoft.EntityFrameworkCore;
using Website.Data;
using Website.Models;
using Website.Models.Dtos;

namespace Website.Services;

public class ApprovalService : IApprovalService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ApprovalService> _logger;

    public ApprovalService(AppDbContext context, ILogger<ApprovalService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> ApproveRecipeAsync(string adminUserId, int recipeId)
    {
        var recipe = await _context.Recipes.FindAsync(recipeId);
        if (recipe == null || recipe.Status != "PendingApproval") return false;

        recipe.Status = "Published";
        recipe.ApprovedBy = adminUserId;
        recipe.ApprovedAt = DateTime.UtcNow;
        recipe.PublishedAt = DateTime.UtcNow;

        var approval = new RecipeApproval
        {
            RecipeId = recipeId,
            ApprovedByUserId = adminUserId,
            ReviewedAt = DateTime.UtcNow,
            Status = "Approved",
            Version = 1 // Simplified versioning for now
        };

        _context.RecipeApprovals.Add(approval);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Recipe {recipeId} approved by admin {adminUserId}");
        return true;
    }

    public async Task<bool> RejectRecipeAsync(string adminUserId, int recipeId, string reason)
    {
        var recipe = await _context.Recipes.FindAsync(recipeId);
        if (recipe == null || recipe.Status != "PendingApproval") return false;

        recipe.Status = "Rejected";
        recipe.RejectionReason = reason;

        var approval = new RecipeApproval
        {
            RecipeId = recipeId,
            ApprovedByUserId = adminUserId,
            ReviewedAt = DateTime.UtcNow,
            Status = "Rejected",
            Reason = reason,
            Version = 1
        };

        _context.RecipeApprovals.Add(approval);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Recipe {recipeId} rejected by admin {adminUserId}. Reason: {reason}");
        return true;
    }

    public async Task<List<PendingRecipeDto>> GetPendingRecipesAsync(int pageNumber = 1, int pageSize = 20)
    {
        return await _context.Recipes
            .Where(r => r.Status == "PendingApproval")
            .OrderBy(r => r.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new PendingRecipeDto
            {
                Id = r.Id,
                Title = r.Title,
                AuthorName = r.Author.DisplayName ?? r.Author.UserName ?? "Anonymous",
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<int> GetPendingRecipeCountAsync()
    {
        return await _context.Recipes.CountAsync(r => r.Status == "PendingApproval");
    }

    public async Task<List<ApprovalHistoryDto>> GetApprovalHistoryAsync(int recipeId)
    {
        return await _context.RecipeApprovals
            .Where(ra => ra.RecipeId == recipeId)
            .OrderByDescending(ra => ra.ReviewedAt)
            .Select(ra => new ApprovalHistoryDto
            {
                Id = ra.Id,
                AdminName = ra.ApprovedBy.DisplayName ?? ra.ApprovedBy.UserName ?? "Admin",
                ReviewedAt = ra.ReviewedAt,
                Status = ra.Status,
                Reason = ra.Reason
            })
            .ToListAsync();
    }
}