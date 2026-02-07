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
        private const int MaxItemQuantity = 15;

        public CartController(IRepository<Product> productRepo)
        {
            _productRepo = productRepo;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(SessionCartKey) ?? new List<CartItem>();
            return View(cart);
        }

        [HttpGet]
        public IActionResult GetCartSidebar()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(SessionCartKey) ?? new List<CartItem>();
            return PartialView("_CartSidebar", cart);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken] // Allow AJAX requests without full page reload
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            var product = _productRepo.Get(p => p.Id == productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Ой! Цей товар кудись подівся... Магія! 🎩" });
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(SessionCartKey) ?? new List<CartItem>();
            var existingItem = cart.FirstOrDefault(c => c.ProductId == productId);

            if (existingItem != null)
            {
                var newQuantity = existingItem.Quantity + quantity;
                if (newQuantity > MaxItemQuantity)
                {
                    return Json(new { 
                        success = false, 
                        message = $"Ого, скільки піц! 🍕 Не забагато? Максимум {MaxItemQuantity} штук!",
                        limitExceeded = true 
                    });
                }
                existingItem.Quantity = newQuantity;
            }
            else
            {
                if (quantity > MaxItemQuantity)
                {
                    return Json(new { 
                        success = false, 
                        message = $"Ого, скільки піц! 🍕 Не забагато? Максимум {MaxItemQuantity} штук!",
                        limitExceeded = true 
                    });
                }
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
                return Json(new { success = true, message = "Смакота додана до кошика! 🛒", cartCount = cartCount });
            }
            
            TempData["Success"] = "Смакота додана до кошика! 🛒";
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

            // AJAX support
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                var cartCount = cart.Sum(c => c.Quantity);
                var cartTotal = cart.Sum(c => c.Price * c.Quantity);
                
                return Json(new { 
                    success = true, 
                    cartCount = cartCount, 
                    cartTotal = cartTotal.ToString("0.00"),
                    isEmpty = !cart.Any()
                });
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
                else if (quantity > MaxItemQuantity)
                {
                    // Return error for limit exceeded
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers["Accept"].ToString().Contains("application/json"))
                    {
                        return Json(new { 
                            success = false, 
                            message = $"Ого, скільки піц! 🍕 Не забагато? Максимум {MaxItemQuantity} штук!",
                            limitExceeded = true,
                            maxQuantity = MaxItemQuantity
                        });
                    }
                    TempData["Error"] = $"Максимум {MaxItemQuantity} одиниць одного товару!";
                    return RedirectToAction("Index");
                }
                else
                {
                    item.Quantity = quantity;
                }
                HttpContext.Session.SetObjectAsJson(SessionCartKey, cart);
            }

            // AJAX support
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                 var cartCount = cart.Sum(c => c.Quantity);
                 var cartTotal = cart.Sum(c => c.Price * c.Quantity);
                 var itemTotal = (item != null && quantity > 0) ? (item.Price * item.Quantity).ToString("0.00") : "0.00";
                 var isRemoved = (item != null && quantity <= 0) || item == null;

                 return Json(new { 
                    success = true, 
                    cartCount = cartCount, 
                    cartTotal = cartTotal.ToString("0.00"),
                    itemTotal = itemTotal,
                    isRemoved = isRemoved,
                    isEmpty = !cart.Any()
                });
            }

            return RedirectToAction("Index");
        }
    }
}
