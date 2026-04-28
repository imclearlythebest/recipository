using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website.Data;
using Website.Models;
using Website.Models.ViewModels;

namespace Website.Controllers;
public class HomeController(AppDbContext dbContext, UserManager<ApplicationUser> userManager) : Controller
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public IActionResult Index(string? feedType = "global")
    {
        var currentUser = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
        var query = _dbContext.Recipes
            .Include(r => r.Author)
            .Include(r => r.Votes)
            .Include(r => r.Reviews)
            .ThenInclude(rv => rv.User)
            .Include(r => r.Replies.OrderByDescending(c => c.CreatedAt))
            .ThenInclude(c => c.Author)
            .Include(r => r.Replies)
            .ThenInclude(c => c.Votes)
            .AsQueryable();

        if (feedType == "followers" && currentUser != null)
        {
            var followedUserIds = _dbContext.Follows
                .Where(f => f.FollowerId == currentUser.Id)
                .Select(f => f.FollowedId)
                .ToList();
            query = query.Where(r => followedUserIds.Contains(r.ApplicationUserId));
        }

        var recipes = query.OrderByDescending(r => r.CreatedAt).ToList();
        // populate user collections for feed picker
        if (currentUser != null)
        {
            var userCollections = _dbContext.Collections
                .Where(c => c.ApplicationUserId == currentUser.Id)
                .OrderBy(c => c.Name)
                .ToList();

            if (!userCollections.Any())
            {
                var defaultCollection = new Collection
                {
                    ApplicationUserId = currentUser.Id,
                    Name = "My Recipes",
                    PublicUrl = Guid.NewGuid().ToString("N")
                };
                _dbContext.Collections.Add(defaultCollection);
                _dbContext.SaveChanges();

                userCollections = _dbContext.Collections
                    .Where(c => c.ApplicationUserId == currentUser.Id)
                    .OrderBy(c => c.Name)
                    .ToList();
            }

            ViewData["UserCollections"] = userCollections;
        }
        else
        {
            ViewData["UserCollections"] = new List<Collection>();
        }

        ViewData["PendingRecipeReviews"] = LoadPendingRecipeReviews(currentUser?.Id);

        return View(recipes);
    }

    public IActionResult Filter(string? search, string? sortBy, string? time, string? feedType)
    {
        // If feedType is passed from checkbox, it's either "followers" or not in the form data at all
        // HTMX doesn't submit unchecked checkboxes, so we need to default to "global"
        if (string.IsNullOrEmpty(feedType))
        {
            feedType = "global";
        }

        var currentUser = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
        var query = _dbContext.Recipes
            .Include(r => r.Author)
            .Include(r => r.Votes)
            .Include(r => r.Reviews)
            .ThenInclude(rv => rv.User)
            .Include(r => r.Replies.OrderByDescending(c => c.CreatedAt))
            .ThenInclude(c => c.Author)
            .Include(r => r.Replies)
            .ThenInclude(c => c.Votes)
            .AsQueryable();

        if (feedType == "followers" && currentUser != null)
        {
            var followedUserIds = _dbContext.Follows
                .Where(f => f.FollowerId == currentUser.Id)
                .Select(f => f.FollowedId)
                .ToList();
            query = query.Where(r => followedUserIds.Contains(r.ApplicationUserId));
        }

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
            "rating" => query.OrderByDescending(r => r.Reviews.Any() ? r.Reviews.Average(rv => rv.Rating) : 0)
                              .ThenByDescending(r => r.Reviews.Count)
                              .ThenByDescending(r => r.CreatedAt),
            "top" => query.OrderByDescending(r => r.Votes.Count(v => v.VoteType == VoteType.Upvote) - 
                                                   r.Votes.Count(v => v.VoteType == VoteType.Downvote))
                           .ThenByDescending(r => r.Reviews.Any() ? r.Reviews.Average(rv => rv.Rating) : 0)
                           .ThenByDescending(r => r.Reviews.Count)
                           .ThenByDescending(r => r.CreatedAt),
            "trending" => query.OrderByDescending(r => r.Votes.Count())
                               .ThenByDescending(r => r.CreatedAt),
            _ => query.OrderByDescending(r => r.CreatedAt)
        };

        var recipes = query.ToList();

        // populate user collections for feed picker (partial requests)
        if (currentUser != null)
        {
            var userCollections = _dbContext.Collections
                .Where(c => c.ApplicationUserId == currentUser.Id)
                .OrderBy(c => c.Name)
                .ToList();

            if (!userCollections.Any())
            {
                var defaultCollection = new Collection
                {
                    ApplicationUserId = currentUser.Id,
                    Name = "My Recipes",
                    PublicUrl = Guid.NewGuid().ToString("N")
                };
                _dbContext.Collections.Add(defaultCollection);
                _dbContext.SaveChanges();

                userCollections = _dbContext.Collections
                    .Where(c => c.ApplicationUserId == currentUser.Id)
                    .OrderBy(c => c.Name)
                    .ToList();
            }

            ViewData["UserCollections"] = userCollections;
        }
        else
        {
            ViewData["UserCollections"] = new List<Collection>();
        }

        ViewData["PendingRecipeReviews"] = LoadPendingRecipeReviews(currentUser?.Id);

        return PartialView("_HomeFeed", recipes);
    }

    public async Task<IActionResult> Post(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        var recipe = await _dbContext.Recipes
            .Include(r => r.Author)
            .Include(r => r.Votes)
            .Include(r => r.Reviews)
            .ThenInclude(rv => rv.User)
            .Include(r => r.Replies.OrderByDescending(c => c.CreatedAt))
            .ThenInclude(c => c.Author)
            .Include(r => r.Replies)
            .ThenInclude(c => c.Votes)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (recipe == null) return NotFound();

        if (currentUser != null)
        {
            var userCollections = await _dbContext.Collections
                .Where(c => c.ApplicationUserId == currentUser.Id)
                .OrderBy(c => c.Name)
                .ToListAsync();

            if (!userCollections.Any())
            {
                // ensure everyone has a default collection
                var defaultCollection = new Collection
                {
                    ApplicationUserId = currentUser.Id,
                    Name = "My Recipes",
                    PublicUrl = Guid.NewGuid().ToString("N")
                };
                _dbContext.Collections.Add(defaultCollection);
                await _dbContext.SaveChangesAsync();

                userCollections = await _dbContext.Collections
                    .Where(c => c.ApplicationUserId == currentUser.Id)
                    .OrderBy(c => c.Name)
                    .ToListAsync();
            }

            ViewData["UserCollections"] = userCollections;
        }
        else
        {
            ViewData["UserCollections"] = new List<Collection>();
        }

        ViewData["PendingRecipeReviews"] = LoadPendingRecipeReviews(currentUser?.Id);

        return View(recipe);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SimulatePurchase(int recipeId, string? returnUrl = null)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var recipe = await _dbContext.Recipes.FindAsync(recipeId);
        if (recipe == null) return NotFound();

        var existingRequest = await _dbContext.RecipeReviewRequests
            .FirstOrDefaultAsync(r => r.ApplicationUserId == currentUser.Id && r.RecipeId == recipeId);

        if (existingRequest == null)
        {
            _dbContext.RecipeReviewRequests.Add(new RecipeReviewRequest
            {
                ApplicationUserId = currentUser.Id,
                RecipeId = recipeId
            });
            await _dbContext.SaveChangesAsync();
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(Post), new { id = recipeId });
    }

    [Authorize]
    public async Task<IActionResult> Review(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var request = await _dbContext.RecipeReviewRequests
            .Include(r => r.Recipe)
            .ThenInclude(recipe => recipe!.Author)
            .FirstOrDefaultAsync(r => r.Id == id && r.ApplicationUserId == currentUser.Id);

        if (request == null) return NotFound();

        return View(request);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(int id, int rating, string reviewText)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        if (rating < 1 || rating > 5)
        {
            ModelState.AddModelError(nameof(rating), "Rating must be between 1 and 5 stars.");
        }

        var request = await _dbContext.RecipeReviewRequests
            .Include(r => r.Recipe)
            .FirstOrDefaultAsync(r => r.Id == id && r.ApplicationUserId == currentUser.Id);

        if (request == null) return NotFound();

        if (!ModelState.IsValid)
        {
            return View(request);
        }

        var existingReview = await _dbContext.RecipeReviews
            .FirstOrDefaultAsync(r => r.ApplicationUserId == currentUser.Id && r.RecipeId == request.RecipeId);

        if (existingReview == null)
        {
            _dbContext.RecipeReviews.Add(new RecipeReview
            {
                ApplicationUserId = currentUser.Id,
                RecipeId = request.RecipeId,
                Rating = rating,
                ReviewText = reviewText,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existingReview.Rating = rating;
            existingReview.ReviewText = reviewText;
            existingReview.CreatedAt = DateTime.UtcNow;
            _dbContext.RecipeReviews.Update(existingReview);
        }

        _dbContext.RecipeReviewRequests.Remove(request);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Post), new { id = request.RecipeId });
    }

    [Authorize]
    public async Task<IActionResult> Reviews()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var pendingReviews = await _dbContext.RecipeReviewRequests
            .Include(r => r.Recipe)
            .ThenInclude(recipe => recipe!.Author)
            .Where(r => r.ApplicationUserId == currentUser.Id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return View(pendingReviews);
    }

    public async Task<IActionResult> Comment(int id)
    {
        var comment = await _dbContext.Contents
            .OfType<Content>()
            .Include(c => c.Author)
            .Include(c => c.Votes)
            .Include(c => c.Parent)
            .Include(c => c.Replies.OrderByDescending(r => r.CreatedAt))
            .ThenInclude(r => r.Author)
            .Include(c => c.Replies)
            .ThenInclude(r => r.Votes)
            .FirstOrDefaultAsync(c => c.Id == id && c.ParentId != null);

        if (comment == null) return NotFound();

        return View(comment);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Upvote(int contentId, string? returnUrl = null)
    {
        var content = await _dbContext.Contents
            .Include(c => c.Votes)
            .FirstOrDefaultAsync(c => c.Id == contentId);

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

        if (Request.Headers["HX-Request"] == "true")
        {
            var updatedContent = await _dbContext.Contents
                .Include(c => c.Votes)
                .FirstAsync(c => c.Id == contentId);

            return PartialView("_VoteControls", CreateVoteControlsViewModel(updatedContent, returnUrl, currentUser.Id));
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Downvote(int contentId, string? returnUrl = null)
    {
        var content = await _dbContext.Contents
            .Include(c => c.Votes)
            .FirstOrDefaultAsync(c => c.Id == contentId);

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

        if (Request.Headers["HX-Request"] == "true")
        {
            var updatedContent = await _dbContext.Contents
                .Include(c => c.Votes)
                .FirstAsync(c => c.Id == contentId);

            return PartialView("_VoteControls", CreateVoteControlsViewModel(updatedContent, returnUrl, currentUser.Id));
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        
        return RedirectToAction(nameof(Index));
    }

    private static VoteControlsViewModel CreateVoteControlsViewModel(Content content, string? returnUrl, string currentUserId)
    {
        var userVote = content.Votes.FirstOrDefault(v => v.ApplicationUserId == currentUserId);

        return new VoteControlsViewModel
        {
            ContentId = content.Id,
            Score = content.Score,
            UserVote = userVote?.VoteType,
            CanVote = true,
            ReturnUrl = returnUrl ?? string.Empty
        };
    }

    private List<RecipeReviewRequest> LoadPendingRecipeReviews(string? currentUserId)
    {
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return new List<RecipeReviewRequest>();
        }

        return _dbContext.RecipeReviewRequests
            .Include(r => r.Recipe)
            .ThenInclude(recipe => recipe!.Author)
            .Where(r => r.ApplicationUserId == currentUserId)
            .OrderByDescending(r => r.CreatedAt)
            .ToList();
    }

    public IActionResult About() => View();
    public IActionResult Contact() => View();
    public IActionResult Privacy() => View();
}