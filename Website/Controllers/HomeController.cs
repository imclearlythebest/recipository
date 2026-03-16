using System.Collections.ObjectModel;
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
    public IActionResult Collection()
    {
        var collections = _dbContext.Collections.ToList();
        return View(collections);
    }

    public IActionResult About() => View();
    public IActionResult Contact() => View();
    public IActionResult Privacy() => View();
}