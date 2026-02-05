using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using CourseWork.Models;
using CourseWork.Repositories;
using CourseWork.Data;
using ProductModel = CourseWork.Models.Product;

namespace CourseWork.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IRepository<ProductModel> _productRepo;
        private readonly IRepository<Category> _categoryRepo;
        private readonly IRepository<Order> _orderRepo;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            IRepository<ProductModel> productRepo,
            IRepository<Category> categoryRepo,
            IRepository<Order> orderRepo,
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _productRepo = productRepo;
            _categoryRepo = categoryRepo;
            _orderRepo = orderRepo;
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            var products = _productRepo.GetAll();
            var categories = _categoryRepo.GetAll();
            var orders = _orderRepo.GetAll();

            ViewBag.ProductsCount = products.Count();
            ViewBag.CategoriesCount = categories.Count();
            ViewBag.OrdersCount = orders.Count();
            ViewBag.TotalRevenue = orders.Where(o => o.OrderStatus == "Paid").Sum(o => o.TotalAmount);

            return View();
        }

        public IActionResult Products()
        {
            var products = _productRepo.GetAll();
            return View(products);
        }

        [HttpGet]
        public IActionResult CreateProduct()
        {
            var categories = _categoryRepo.GetAll();
            ViewBag.Categories = categories;
            ViewBag.Restaurants = _db.Restaurants.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateProduct(ProductModel product)
        {
            if (ModelState.IsValid)
            {
                _productRepo.Add(product);
                _db.SaveChanges();
                TempData["Success"] = "Товар успішно додано!";
                return RedirectToAction("Products");
            }
            var categories = _categoryRepo.GetAll();
            ViewBag.Categories = categories;
            ViewBag.Restaurants = _db.Restaurants.ToList();
            return View(product);
        }

        [HttpGet]
        public IActionResult EditProduct(int id)
        {
            var product = _productRepo.Get(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            var categories = _categoryRepo.GetAll();
            ViewBag.Categories = categories;
            ViewBag.Restaurants = _db.Restaurants.ToList();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProduct(ProductModel product)
        {
            if (ModelState.IsValid)
            {
                _db.Products.Update(product);
                _db.SaveChanges();
                TempData["Success"] = "Товар успішно оновлено!";
                return RedirectToAction("Products");
            }
            var categories = _categoryRepo.GetAll();
            ViewBag.Categories = categories;
            ViewBag.Restaurants = _db.Restaurants.ToList();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteProduct(int id)
        {
            var product = _productRepo.Get(p => p.Id == id);
            if (product != null)
            {
                _productRepo.Remove(product);
                _db.SaveChanges();
                TempData["Success"] = "Товар успішно видалено!";
            }
            return RedirectToAction("Products");
        }

        public IActionResult Orders()
        {
            var orders = _orderRepo.GetAll();
            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderStatus(int orderId, string status)
        {
            var order = _orderRepo.Get(o => o.Id == orderId);
            if (order != null)
            {
                order.OrderStatus = status;
                _db.Orders.Update(order);
                _db.SaveChanges();
                TempData["Success"] = "Статус замовлення оновлено!";
            }
            return RedirectToAction("Orders");
        }

        public async Task<IActionResult> Users()
        {
            var users = _userManager.Users.ToList();
            var userRoles = new Dictionary<string, List<string>>();
            
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.ToList();
            }
            
            ViewBag.UserRoles = userRoles;
            ViewBag.AllRoles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdminRole(string userId, bool isAdmin)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            if (isAdmin)
            {
                if (!await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                    TempData["Success"] = "Роль адміністратора додано!";
                }
            }
            else
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    await _userManager.RemoveFromRoleAsync(user, "Admin");
                    TempData["Success"] = "Роль адміністратора видалено!";
                }
            }

            return RedirectToAction("Users");
        }
    }
}

