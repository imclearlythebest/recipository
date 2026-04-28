using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website.Data;
using Website.Models;


namespace Website.Controllers;

public class UserController(AppDbContext dbContext, UserManager<ApplicationUser> userManager) : Controller
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<IActionResult> Profile(string username)
    {
        var user = await _dbContext.Users
            .Include(u => u.Followers)
            .Include(u => u.Following)
            .FirstOrDefaultAsync(u => u.UserName == username);

        if (user == null) return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);
        
        // Pass follow status and whether viewing own profile
        ViewData["CurrentUserId"] = currentUser?.Id;
        ViewData["IsOwnProfile"] = currentUser?.Id == user.Id;
        
        if (currentUser != null)
        {
            var isFollowing = await _dbContext.Follows
                .AnyAsync(f => f.FollowerId == currentUser.Id && f.FollowedId == user.Id);
            ViewData["IsFollowing"] = isFollowing;
        }

        return View(user);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Follow(string username)
    {
        var targetUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == username);
        if (targetUser == null) return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        // Can't follow yourself
        if (currentUser.Id == targetUser.Id)
            return BadRequest("You cannot follow yourself.");

        // Check if already following
        var existingFollow = await _dbContext.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == currentUser.Id && f.FollowedId == targetUser.Id);

        if (existingFollow == null)
        {
            var follow = new Follow
            {
                FollowerId = currentUser.Id,
                FollowedId = targetUser.Id
            };
            _dbContext.Follows.Add(follow);
            await _dbContext.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Profile), new { username });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Unfollow(string username)
    {
        var targetUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == username);
        if (targetUser == null) return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var follow = await _dbContext.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == currentUser.Id && f.FollowedId == targetUser.Id);

        if (follow != null)
        {
            _dbContext.Follows.Remove(follow);
            await _dbContext.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Profile), new { username });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        return View(user);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Edit(ApplicationUser model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        if (user.Id != model.Id) return BadRequest();

        if (!ModelState.IsValid)
            return View(user);

        user.DisplayName = model.DisplayName;
        user.Bio = model.Bio;
        user.AvatarUrl = model.AvatarUrl;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            return RedirectToAction(nameof(Profile), new { username = user.UserName });
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(user);
    }
}
