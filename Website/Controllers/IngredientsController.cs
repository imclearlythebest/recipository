using Microsoft.AspNetCore.Mvc;
using Website.Data;
using Website.Models;


namespace Website.Controllers;

public class IngredientsController(AppDbContext dbContext) : Controller
{
    private readonly AppDbContext _dbContext = dbContext;

    public IActionResult Index()
    {
        var ingredients = _dbContext.Ingredients.ToList();
        return View(ingredients);
    }

}
