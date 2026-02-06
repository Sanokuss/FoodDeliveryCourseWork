using System.Diagnostics;
using CourseWork.Models;
using CourseWork.Repositories;
using CourseWork.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            IEnumerable<Product> products = _productRepo.GetAll(includeProperties: "Category,Restaurant").ToList(); // Materialize to memory for smart search
            
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
            // SMART SEARCH LOGIC 🧠
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchTerms = new List<string>();
                string originalTerm = searchTerm.Trim().ToLower();
                searchTerms.Add(originalTerm);

                // 1. Keyboard Layout Fix (e.g., "ghbds" -> "привіт", "gbwwf" -> "піцца")
                string fixedLayoutTerm = KeyboardLayoutConverter.FixLayout(originalTerm);
                if (!string.Equals(fixedLayoutTerm, originalTerm, StringComparison.OrdinalIgnoreCase))
                {
                    searchTerms.Add(fixedLayoutTerm);
                }

                // 2. Dictionary Mapping (English/Russian -> Ukrainian)
                var mapping = new Dictionary<string, string> {
                    { "pizza", "піца" }, { "пицца", "піца" }, 
                    { "burger", "бургер" }, { "бургеры", "бургер" },
                    { "sushi", "суші" }, { "суши", "суші" }, { "роллы", "суші" },
                    { "salat", "салат" }, { "drink", "напої" }, 
                    { "coke", "coca-cola" }, { "pepsi", "coca-cola" }, 
                    { "margarita", "маргарита" }, { "pepperoni", "пепероні" },
                    { "cheese", "сир" }, { "water", "вода" }, { "вода", "вода" }
                };

                // Check mappings for all current terms
                var mappedTerms = new List<string>();
                foreach (var term in searchTerms)
                {
                    foreach (var key in mapping.Keys)
                    {
                        // Use Fuzzy matching for dictionary keys too! (e.g. "pizaa" -> match "pizza" -> map to "піца")
                        if (FuzzySharp.Fuzz.Ratio(term, key) > 85 || term.Contains(key)) 
                        {
                            mappedTerms.Add(mapping[key]);
                        }
                    }
                }
                searchTerms.AddRange(mappedTerms);
                searchTerms = searchTerms.Distinct().ToList();

                // 3. Perform Fuzzy Search against Products
                // We score each product against the BEST matching search term
                var searchResults = products.Select(p => new
                {
                    Product = p,
                    Score = searchTerms.Max(term => 
                    {
                        var nameScore = Math.Max(
                            FuzzySharp.Fuzz.PartialRatio(term, p.Name.ToLower()), 
                            FuzzySharp.Fuzz.Ratio(term, p.Name.ToLower())
                        );
                        
                        var categoryScore = p.Category != null ? 
                            FuzzySharp.Fuzz.PartialRatio(term, p.Category.Name.ToLower()) : 0;
                            
                        var descScore = p.Description != null ? 
                            FuzzySharp.Fuzz.PartialRatio(term, p.Description.ToLower()) : 0;

                        // Prioritize Name matches, then Category, then Description
                        return Math.Max(nameScore, Math.Max(categoryScore, descScore));
                    })
                })
                .Where(x => x.Score > 60) // Threshold for "good enough" match
                .OrderByDescending(x => x.Score)
                .Select(x => x.Product)
                .ToList();

                if (searchResults.Any())
                {
                    products = searchResults;
                }
                else
                {
                    products = new List<Product>();
                }
            }

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
