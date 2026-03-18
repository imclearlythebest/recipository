using Microsoft.AspNetCore.Mvc;
using Website.Models; 

namespace Website.Controllers
{
    public class MarketplaceController : Controller
    {
        public IActionResult Index()
        {
            var marketplaceItems = new List<ShopItem>
            {
                new ShopItem { 
                    Id = 1, 
                    Price = 12.99m, 
                    Stock = 10, 
                    Ingredient = new Ingredient("Fresh Avocado", "Creamy and ripe", "avocado.jpg", 160) 
                },
                new ShopItem { 
                    Id = 2, 
                    Price = 3.50m, 
                    Stock = 100, 
                    Ingredient = new Ingredient("Whole Wheat Bread", "Freshly baked", "bread.jpg", 250) 
                }
            };

            return View(marketplaceItems);
        }

        [HttpPost]
        public IActionResult AddToCart(int shopItemId, int quantity)
        {

            return RedirectToAction("Index", "Cart");
        }
    }
}