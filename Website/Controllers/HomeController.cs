using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website.Data;
using Website.Models;

namespace Website.Controllers;
public class HomeController(AppDbContext dbContext) : Controller
{
    private readonly AppDbContext _dbContext = dbContext;

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
    public IActionResult About() => View();
    public IActionResult Contact() => View();
    public IActionResult Privacy() => View();
}