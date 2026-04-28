using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website.Data;
using Website.Models;

namespace Website.Controllers;

public class ContentController(AppDbContext dbContext, UserManager<ApplicationUser> userManager) : Controller
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddComment(int recipeId, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return BadRequest("Comment cannot be empty");

        var recipe = await _dbContext.Recipes.FindAsync(recipeId);
        if (recipe == null) return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var comment = new Content
        {
            ApplicationUserId = currentUser.Id,
            MainText = text,
            CreatedAt = DateTime.UtcNow,
            ParentId = recipeId
        };

        _dbContext.Contents.Add(comment);
        await _dbContext.SaveChangesAsync();
        if (Request.Headers["HX-Request"] == "true")
        {
            var recipeWithReplies = await _dbContext.Recipes
                .Include(r => r.Replies.OrderByDescending(c => c.CreatedAt))
                .ThenInclude(c => c.Author)
                .Include(r => r.Replies)
                .ThenInclude(c => c.Votes)
                .FirstOrDefaultAsync(r => r.Id == recipeId);

            if (recipeWithReplies == null) return NotFound();

            return PartialView("~/Views/Home/_CommentsList.cshtml", recipeWithReplies.Replies.OrderByDescending(c => c.CreatedAt).ToList());
        }

        return RedirectToAction("Post", "Home", new { id = recipeId });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddReply(int parentId, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return BadRequest("Reply cannot be empty");

        var parent = await _dbContext.Contents.Include(c => c.Parent).FirstOrDefaultAsync(c => c.Id == parentId);
        if (parent == null) return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var reply = new Content
        {
            ApplicationUserId = currentUser.Id,
            MainText = text,
            CreatedAt = DateTime.UtcNow,
            ParentId = parentId
        };

        _dbContext.Contents.Add(reply);
        await _dbContext.SaveChangesAsync();
        if (Request.Headers["HX-Request"] == "true")
        {
            var parentWithReplies = await _dbContext.Contents
                .Include(c => c.Replies.OrderByDescending(r => r.CreatedAt))
                .ThenInclude(r => r.Author)
                .Include(c => c.Replies)
                .ThenInclude(r => r.Votes)
                .FirstOrDefaultAsync(c => c.Id == parentId);

            if (parentWithReplies == null) return NotFound();

            return PartialView("~/Views/Home/_CommentsList.cshtml", parentWithReplies.Replies.OrderByDescending(c => c.CreatedAt).ToList());
        }

        return RedirectToAction("Comment", "Home", new { id = parentId });
    }
}
