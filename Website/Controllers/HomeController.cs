using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website.Data;

namespace Website.Controllers;
public class HomeController(AppDbContext dbContext) : Controller
{
    private readonly AppDbContext _dbContext = dbContext;

    public IActionResult Index()
    {
        var recipes = _dbContext.Recipes.Include(r => r.Author).ToList();
        return View(recipes);

    }
    public IActionResult About() => View();
    public IActionResult Contact() => View();
    public IActionResult Privacy() => View();
}