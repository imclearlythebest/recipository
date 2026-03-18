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
                new ShopItem("Organic Avocado", "Ripe and ready for toast", "avocado.jpg", 160f, 2.50f, 50, 1, 1),
                new ShopItem("Sea Salt", "Fine grain Mediterranean salt", "salt.jpg", 0f, 1.20f, 100, 1, 1),
                new ShopItem("Extra Virgin Olive Oil", "Cold pressed 500ml", "oil.jpg", 120f, 15.00f, 20, 1, 1)
            };

            return View(marketplaceItems);
        }

        [HttpPost]
        public IActionResult AddToCart(int id, int quantity)
        {

            System.Diagnostics.Debug.WriteLine($"Added item {id} with quantity {quantity}");
            
            return RedirectToAction("Index"); 
        }
    }
}