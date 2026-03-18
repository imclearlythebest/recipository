using Microsoft.AspNetCore.Mvc;
using Website.Models;

namespace Website.Controllers;

public class CartController : Controller
{
    private static List<CartItem> _cart = new List<CartItem>();

    public IActionResult Index()
    {
        return View(_cart);
    }

    [HttpPost]
    public IActionResult AddToCart(int id, int quantity = 1)
    {
        var existingItem = _cart.FirstOrDefault(x => x.ShopItemId == id);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {

            _cart.Add(new CartItem { 
                ShopItemId = id, 
                Quantity = quantity,
                Product = new ShopItem { Name = "Selected Item", Price = 10.00f } 
            });
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Remove(int id)
    {
        _cart.RemoveAll(x => x.ShopItemId == id);
        return RedirectToAction("Index");
    }
}