using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website.Data;
using Website.Models;

namespace Website.Controllers;
public class HomeController(AppDbContext dbContext, UserManager<ApplicationUser> userManager) : Controller
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public IActionResult Index()
    {
        var recipes = _dbContext.Recipes
            .Include(r => r.Author)
            .Include(r => r.Votes)
            .OrderByDescending(r => r.CreatedAt)
            .ToList();
        return View(recipes);
    }

    public IActionResult Filter(string? search, string? sortBy, string? time)
    {
        var query = _dbContext.Recipes
            .Include(r => r.Author)
            .Include(r => r.Votes)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            var lowSearch = search.ToLower();
            query = query.Where(r => r.Title.ToLower().Contains(lowSearch) || 
                                     r.MainText.ToLower().Contains(lowSearch));
        }

        if (!string.IsNullOrEmpty(time) && time != "all")
        {
            var now = DateTime.UtcNow;
            query = time switch
            {
                "today" => query.Where(r => r.CreatedAt >= now.Date),
                "week" => query.Where(r => r.CreatedAt >= now.AddDays(-7)),
                _ => query
            };
        }

        query = sortBy switch
        {
            "top" => query.OrderByDescending(r => r.Votes.Count(v => v.VoteType == VoteType.Upvote) - 
                                                   r.Votes.Count(v => v.VoteType == VoteType.Downvote)),
            "trending" => query.OrderByDescending(r => r.Votes.Count())
                               .ThenByDescending(r => r.CreatedAt),
            _ => query.OrderByDescending(r => r.CreatedAt)
        };

        var recipes = query.ToList();
        return PartialView("_HomeFeed", recipes);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Upvote(int contentId)
    {
        var content = await _dbContext.Recipes
            .Include(r => r.Votes)
            .FirstOrDefaultAsync(r => r.Id == contentId);

        if (content == null) return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var existingVote = content.Votes.FirstOrDefault(v => v.ApplicationUserId == currentUser.Id);

        if (existingVote == null)
        {
            // No vote exists, create upvote
            var vote = new ContentVote
            {
                ApplicationUserId = currentUser.Id,
                ContentId = contentId,
                VoteType = VoteType.Upvote
            };
            _dbContext.ContentVotes.Add(vote);
        }
        else if (existingVote.VoteType == VoteType.Upvote)
        {
            // Already upvoted, remove vote (toggle)
            _dbContext.ContentVotes.Remove(existingVote);
        }
        else
        {
            // Was downvoted, switch to upvote
            existingVote.VoteType = VoteType.Upvote;
            _dbContext.ContentVotes.Update(existingVote);
        }

        await _dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Downvote(int contentId)
    {
        var content = await _dbContext.Recipes
            .Include(r => r.Votes)
            .FirstOrDefaultAsync(r => r.Id == contentId);

        if (content == null) return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var existingVote = content.Votes.FirstOrDefault(v => v.ApplicationUserId == currentUser.Id);

        if (existingVote == null)
        {
            // No vote exists, create downvote
            var vote = new ContentVote
            {
                ApplicationUserId = currentUser.Id,
                ContentId = contentId,
                VoteType = VoteType.Downvote
            };
            _dbContext.ContentVotes.Add(vote);
        }
        else if (existingVote.VoteType == VoteType.Downvote)
        {
            // Already downvoted, remove vote (toggle)
            _dbContext.ContentVotes.Remove(existingVote);
        }
        else
        {
            // Was upvoted, switch to downvote
            existingVote.VoteType = VoteType.Downvote;
            _dbContext.ContentVotes.Update(existingVote);
        }

        await _dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public IActionResult About() => View();
    public IActionResult Contact() => View();
    public IActionResult Privacy() => View();
}