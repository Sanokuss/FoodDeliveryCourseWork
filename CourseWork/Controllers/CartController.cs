using Microsoft.AspNetCore.Mvc;
using CourseWork.Models;
using CourseWork.Repositories;
using CourseWork.ViewModels;
using CourseWork.Utility;

namespace CourseWork.Controllers
{
    public class CartController : Controller
    {
        private readonly IRepository<Product> _productRepo;
        private const string SessionCartKey = "ShoppingCart";

        public CartController(IRepository<Product> productRepo)
        {
            _productRepo = productRepo;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(SessionCartKey) ?? new List<CartItem>();
            return View(cart);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken] // Allow AJAX requests without full page reload
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            var product = _productRepo.Get(p => p.Id == productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Товар не знайдено" });
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(SessionCartKey) ?? new List<CartItem>();
            var existingItem = cart.FirstOrDefault(c => c.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    ImageUrl = product.ImageUrl
                });
            }

            HttpContext.Session.SetObjectAsJson(SessionCartKey, cart);
            var cartCount = cart.Sum(c => c.Quantity);
            
            // Return JSON for AJAX requests, or redirect for regular form submissions
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                return Json(new { success = true, message = "Товар додано до кошика!", cartCount = cartCount });
            }
            
            TempData["Success"] = "Товар додано до кошика!";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(SessionCartKey) ?? new List<CartItem>();
            var itemToRemove = cart.FirstOrDefault(c => c.ProductId == productId);

            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                HttpContext.Session.SetObjectAsJson(SessionCartKey, cart);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(SessionCartKey) ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);

            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
                HttpContext.Session.SetObjectAsJson(SessionCartKey, cart);
            }

            return RedirectToAction("Index");
        }
    }
}

