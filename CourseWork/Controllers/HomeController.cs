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

        public IActionResult Index(int? categoryId, string searchTerm = "", string sortOrder = "")
        {
            var categories = _categoryRepo.GetAll().OrderBy(c => c.DisplayOrder).ToList();
            ViewBag.Categories = categories;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SelectedCategoryId = categoryId;

            // Start with all products
            IEnumerable<Product> products = _productRepo.GetAll().ToList(); // Materialize to memory for smart search
            
            // Filter by category
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value).ToList();
            }

            // SORTING LOGIC
            // Default sort is DisplayOrder (if categories) or generally by ID/Name if not specified
            // But we apply explicit sort if sortOrder is provided
            ViewBag.CurrentSort = sortOrder; // Pass back to view

            switch (sortOrder)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.Price).ToList();
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price).ToList();
                    break;
                case "newest":
                    products = products.OrderByDescending(p => p.IsNew).ThenBy(p => p.Name).ToList();
                    break;
                case "bestseller":
                    products = products.OrderByDescending(p => p.IsBestSeller).ThenBy(p => p.Name).ToList();
                    break;
                default:
                    // Default logic (e.g. Best Sellers first, then New, then others? Or just random/ID)
                    // Let's keep existing order (Default) unless specified
                    break;
            }

            // SMART SEARCH LOGIC 🧠
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string term = searchTerm.Trim().ToLower();
                
                // 1. Standard "Contains" search
                var directMatches = products.Where(p => 
                    p.Name.ToLower().Contains(term) || 
                    (p.Description != null && p.Description.ToLower().Contains(term)) ||
                    (p.Category != null && p.Category.Name.ToLower().Contains(term))
                ).ToList();

                if (directMatches.Any())
                {
                    products = directMatches;
                }
                else
                {
                    // 2. Try Dictionary Mapping (English -> Ukrainian)
                    var mapping = new Dictionary<string, string> {
                        { "pizza", "піца" }, { "burger", "бургер" }, { "sushi", "суші" }, 
                        { "salat", "салат" }, { "drink", "напої" }, { "coke", "coca-cola" },
                        { "pepsi", "coca-cola" }, { "margarita", "маргарита" }, { "pepperoni", "пепероні" },
                        { "cheese", "сир" }, { "water", "вода" }
                    };

                    string mappedTerm = null;
                    foreach(var key in mapping.Keys)
                    {
                        if (term.Contains(key)) 
                        {
                            mappedTerm = mapping[key];
                            break;
                        }
                    }

                    if (mappedTerm != null)
                    {
                        products = products.Where(p => 
                            p.Name.ToLower().Contains(mappedTerm) || 
                            (p.Category != null && p.Category.Name.ToLower().Contains(mappedTerm))
                        ).ToList();
                    }
                    else
                    {
                        // 3. Fuzzy Search (Levenshtein) - Last Resort for Typos
                        var fuzzyMatches = products.Where(p => 
                            ComputeLevenshteinDistance(term, p.Name.ToLower()) <= 3 // Allow up to 3 errors
                        ).ToList();

                        if (fuzzyMatches.Any())
                        {
                            products = fuzzyMatches;
                        }
                        else
                        {
                            products = new List<Product>(); // No results found
                        }
                    }
                }
            }

            // Include category for display (already in memory objects usually, but kept for consistency)
            // products is already List<Product>

            // Get cart count for badge (Keep existing logic)
            var cart = HttpContext.Session.GetObjectFromJson<List<ViewModels.CartItem>>(SessionCartKey) ?? new List<ViewModels.CartItem>();
            ViewBag.CartCount = cart.Sum(c => c.Quantity);

            // Return partial view for AJAX requests
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ProductList", products);
            }

            return View(products);
        }

        // Helper: Levenshtein Distance
        private static int ComputeLevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            var d = new int[n + 1, m + 1];

            if (n == 0) return m;
            if (m == 0) return n;

            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }

        [HttpGet]
        public IActionResult GetProductDetails(int id)
        {
            var product = _productRepo.GetAll(includeProperties: "Category,Restaurant")
                            .FirstOrDefault(p => p.Id == id);
            
            if (product == null)
            {
                return NotFound();
            }

            return PartialView("_ProductDetailsModal", product);
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
