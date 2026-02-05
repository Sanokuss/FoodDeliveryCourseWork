using System.Diagnostics;
using CourseWork.Models;
using CourseWork.Repositories;
using CourseWork.Utility;
using Microsoft.AspNetCore.Mvc;

namespace CourseWork.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<Category> _categoryRepo;
        private const string SessionCartKey = "ShoppingCart";

        public HomeController(
            ILogger<HomeController> logger,
            IRepository<Product> productRepo,
            IRepository<Category> categoryRepo)
        {
            _logger = logger;
            _productRepo = productRepo;
            _categoryRepo = categoryRepo;
        }

        public IActionResult Index(int? categoryId, string searchTerm = "")
        {
            var categories = _categoryRepo.GetAll().OrderBy(c => c.DisplayOrder).ToList();
            ViewBag.Categories = categories;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SelectedCategoryId = categoryId;

            IEnumerable<Product> products = _productRepo.GetAll();

            // Filter by category
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            // Filter by search term (Name, Description, and Category)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerTerm = searchTerm.Trim().ToLower();
                products = products.Where(p => 
                    p.Name.ToLower().Contains(lowerTerm) || 
                    (p.Description != null && p.Description.ToLower().Contains(lowerTerm)) ||
                    (p.Category != null && p.Category.Name.ToLower().Contains(lowerTerm)));
            }

            // Include category for display
            products = products.ToList();

            // Get cart count for badge
            var cart = HttpContext.Session.GetObjectFromJson<List<ViewModels.CartItem>>(SessionCartKey) ?? new List<ViewModels.CartItem>();
            ViewBag.CartCount = cart.Sum(c => c.Quantity);

            // Return partial view for AJAX requests
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ProductList", products);
            }

            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
