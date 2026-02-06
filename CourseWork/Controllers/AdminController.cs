using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using CourseWork.Models;
using CourseWork.Repositories;
using CourseWork.Data;
using ProductModel = CourseWork.Models.Product;

namespace CourseWork.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
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

        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleRole(string userId, string role, bool enable)
        {
            if (role != "Admin" && role != "Manager")
            {
                TempData["Error"] = $"Невідома роль: {role}";
                return RedirectToAction("Users");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "Користувача не знайдено";
                return RedirectToAction("Users");
            }

            // --- SECURITY FIX: Prevent removing admin role from Main Admin ---
            if (!enable && role == "Admin" && (user.Email == "admin@fooddelivery.com" || user.UserName == "admin@fooddelivery.com"))
            {
                TempData["Error"] = "Не можна забрати права у головного адміністратора!";
                return RedirectToAction("Users");
            }

            IdentityResult result = IdentityResult.Success;
            if (enable)
            {
                if (!await _userManager.IsInRoleAsync(user, role))
                {
                    result = await _userManager.AddToRoleAsync(user, role);
                    if (result.Succeeded)
                    {
                        TempData["Success"] = $"Роль {role} успішно додано!";
                    }
                }
                else
                {
                    TempData["Error"] = $"Користувач вже має роль {role} (дія не виконана)";
                }
            }
            else
            {
                if (await _userManager.IsInRoleAsync(user, role))
                {
                    result = await _userManager.RemoveFromRoleAsync(user, role);
                    if (result.Succeeded)
                    {
                        TempData["Success"] = $"Роль {role} успішно видалено!";
                    }
                }
                 else
                {
                    TempData["Error"] = $"Користувач не має ролі {role} (дія не виконана)";
                }
            }

            if (!result.Succeeded)
            {
                TempData["Error"] = $"Помилка: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }

            return RedirectToAction("Users");
        }
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, string fullName)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.FullName = fullName;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["Success"] = "Дані користувача оновлено!";
                return RedirectToAction("Users");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(user);
        }
    }
}

