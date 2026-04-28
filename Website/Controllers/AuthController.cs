using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Website.Models;
using Website.Models.Dtos;
using Website.Data;


namespace Website.Controllers;

public class AuthController: Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly AppDbContext _dbContext;
    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        AppDbContext dbContext)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
    }
    [HttpGet]
    public IActionResult Login() => View();
    [HttpPost]
    public async Task<IActionResult> Login(LoginDto model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, isPersistent: true, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            return RedirectToAction("Index", "Home");
        }
        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }
    [HttpGet]
    public IActionResult Register() => View();
    [HttpPost]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = new ApplicationUser
        {
            UserName = model.Username,
            Email = model.Email,
            DisplayName = model.DisplayName
        };
        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            // Create a default collection for new users
            var defaultCollection = new Collection
            {
                ApplicationUserId = user.Id,
                Name = "My Recipes",
                PublicUrl = Guid.NewGuid().ToString("N")
            };

            _dbContext.Collections.Add(defaultCollection);
            await _dbContext.SaveChangesAsync();

            await _signInManager.SignInAsync(user, isPersistent: true);
            return RedirectToAction("Index", "Home");
        }
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        return View(model);
    }
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
    public IActionResult Denied()
    {
        return View();
    }
}