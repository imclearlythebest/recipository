using Microsoft.AspNetCore.Mvc;
using Website.Data;

namespace Website.Controllers;
public class HomeController(AppDbContext dbContext) : Controller
{
    private readonly AppDbContext _dbContext = dbContext;

    public IActionResult Index()
    {
        var contents = _dbContext.Contents.ToList();
        return View(contents);

    }
    public IActionResult About() => View();
    public IActionResult Contact() => View();
    public IActionResult Privacy() => View();
}