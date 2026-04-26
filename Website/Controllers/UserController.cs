using Microsoft.AspNetCore.Mvc;
using Website.Data;
using Website.Models;


namespace Website.Controllers;

public class UserController(AppDbContext dbContext) : Controller
{
    private readonly AppDbContext _dbContext = dbContext;

    public IActionResult Profile(string username)
    {
        var User = _dbContext.Users.FirstOrDefault<ApplicationUser>(u => u.UserName == username);
        return View(User);
    }

}
