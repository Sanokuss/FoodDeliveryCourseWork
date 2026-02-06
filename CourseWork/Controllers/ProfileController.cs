using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CourseWork.Models;
using CourseWork.ViewModels;
using CourseWork.Repositories;

namespace CourseWork.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrderRepository _orderRepository;
        private readonly IRepository<Promotion> _promotionRepository;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            IOrderRepository orderRepository,
            IRepository<Promotion> promotionRepository)
        {
            _userManager = userManager;
            _orderRepository = orderRepository;
            _promotionRepository = promotionRepository;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get user's orders
            var orders = _orderRepository.GetAll()
                .Where(o => o.ApplicationUserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            // Calculate total spent
            var totalSpent = orders.Sum(o => o.TotalAmount);

            // Get active promotions
            var promotions = _promotionRepository.GetAll()
                .Where(p => p.IsActive)
                .Take(3)
                .ToList();

            // Determine user level based on spending
            string userLevel = "Новачок";
            int discountPercent = 0;
            if (totalSpent >= 5000)
            {
                userLevel = "VIP";
                discountPercent = 15;
            }
            else if (totalSpent >= 2000)
            {
                userLevel = "Золотий";
                discountPercent = 10;
            }
            else if (totalSpent >= 500)
            {
                userLevel = "Срібний";
                discountPercent = 5;
            }

            ViewBag.User = user;
            ViewBag.Orders = orders;
            ViewBag.TotalSpent = totalSpent;
            ViewBag.Promotions = promotions;
            ViewBag.UserLevel = userLevel;
            ViewBag.DiscountPercent = discountPercent;
            ViewBag.OrderCount = orders.Count;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new ProfileEditViewModel
            {
                FullName = user.FullName ?? string.Empty,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            user.FullName = model.FullName;
            user.Address = model.Address;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Профіль успішно оновлено! 🎉";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
    }
}
