using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using CourseWork.Models;
using CourseWork.Repositories;
using CourseWork.ViewModels;
using CourseWork.Utility;
using CourseWork.Data;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;
using Stripe;
using ProductModel = CourseWork.Models.Product;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace CourseWork.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IRepository<ProductModel> _productRepo;
        private readonly IRepository<OrderDetail> _orderDetailRepo;
        private readonly ApplicationDbContext _db;
        private const string SessionCartKey = "ShoppingCart";

        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(
            IOrderRepository orderRepo,
            IRepository<ProductModel> productRepo,
            IRepository<OrderDetail> orderDetailRepo,
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager)
        {
            _orderRepo = orderRepo;
            _productRepo = productRepo;
            _orderDetailRepo = orderDetailRepo;
            _db = db;
            _userManager = userManager;
        }

        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(SessionCartKey) ?? new List<CartItem>();
            if (!cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var model = new CheckoutViewModel();
            decimal cartTotal = cart.Sum(c => c.Price * c.Quantity);
            model.TotalAmount = cartTotal;
            model.DiscountAmount = 0;

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    model.CustomerName = user.FullName ?? "";
                    model.CustomerPhone = user.PhoneNumber ?? "";
                    model.CustomerAddress = user.Address ?? "";
                    model.UserTotalSpent = user.TotalSpent;

                    // Calculate Loyalty Discount: 1% per 1000 UAH spent, max 10%
                    int loyaltyPercent = Math.Min(10, (int)(user.TotalSpent / 1000));
                    model.LoyaltyDiscountPercent = loyaltyPercent;
                    
                    if (loyaltyPercent > 0)
                    {
                        model.LoyaltyDiscountAmount = cartTotal * (loyaltyPercent / 100m);
                        // Default: toggle OFF, user must enable to apply
                        model.UsePersonalDiscount = false;
                        model.DiscountAmount = 0;
                        model.TotalAmount = cartTotal;
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Checkout", model);
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(SessionCartKey) ?? new List<CartItem>();
            if (!cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            // Create Order
            var order = new Order
            {
                CustomerName = model.CustomerName,
                CustomerPhone = model.CustomerPhone,
                CustomerAddress = model.CustomerAddress,
                OrderDate = DateTime.Now,
                OrderStatus = "Pending"
            };

            decimal cartTotal = cart.Sum(c => c.Price * c.Quantity);
            decimal discountAmount = 0;

            // Link to User if authenticated & Apply Loyalty Discount
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    order.ApplicationUserId = user.Id;

                    // Calculate Loyalty Discount: 1% per 1000 UAH spent, max 10%
                    if (model.UsePersonalDiscount)
                    {
                        int loyaltyPercent = Math.Min(10, (int)(user.TotalSpent / 1000));
                        if (loyaltyPercent > 0)
                        {
                            discountAmount = cartTotal * (loyaltyPercent / 100m);
                        }
                    }

                    // Update User Profile
                    if (string.IsNullOrEmpty(user.FullName)) { user.FullName = model.CustomerName; }
                    if (string.IsNullOrEmpty(user.PhoneNumber)) { user.PhoneNumber = model.CustomerPhone; }
                    if (string.IsNullOrEmpty(user.Address)) { user.Address = model.CustomerAddress; }
                    
                    // Update TotalSpent with final amount (after discount)
                    decimal finalAmount = cartTotal - discountAmount;
                    user.TotalSpent += finalAmount;

                    await _userManager.UpdateAsync(user);
                }
            }

            order.TotalAmount = cartTotal - discountAmount;
            order.DiscountAmount = discountAmount;

            _orderRepo.Add(order);
            _db.SaveChanges(); // Save to generate ID

            // Create OrderDetails
            foreach (var item in cart)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price
                };
                _orderDetailRepo.Add(orderDetail);
            }

            _db.SaveChanges();

            // Handle Payment
            if (model.PaymentMethod == "Cash")
            {
                 // Cash payment - skip Stripe
                 return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
            }

            // Create Stripe Session
            try
            {
                var stripeSecretKey = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Stripe:SecretKey"];
                
                // Check if Stripe key is configured
                if (string.IsNullOrEmpty(stripeSecretKey) || stripeSecretKey.Contains("your_secret_key"))
                {
                    ModelState.AddModelError("", "Оплата прилягла відпочити 💤 Сервіс тимчасово недоступний. Оберіть оплату готівкою!");
                    return View("Checkout", model);
                }

                var domain = $"{Request.Scheme}://{Request.Host}";
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = cart.Select(item => new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100), // Convert to cents
                            Currency = "uah",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Name,
                                Images = new List<string> { item.ImageUrl }
                            }
                        },
                        Quantity = item.Quantity
                    }).ToList(),
                    Mode = "payment",
                    SuccessUrl = $"{domain}/Order/OrderConfirmation?orderId={order.Id}",
                    CancelUrl = $"{domain}/Order/OrderFailure?orderId={order.Id}"
                };

                var service = new SessionService();
                Session session = service.Create(options);

                order.TransactionId = session.Id;
                _orderRepo.Update(order);
                _db.SaveChanges();

                Response.Headers["Location"] = session.Url;
                return new StatusCodeResult(303);
            }
            catch (StripeException ex)
            {
                // Log the error (in production, use proper logging)
                ModelState.AddModelError("", $"Платіжна система вередує! 💳 {ex.Message}. Спробуйте готівку!");
                return View("Checkout", model);
            }
            catch (Exception)
            {
                // Log the error (in production use proper logging)
                ModelState.AddModelError("", "Щось пішло не так... Навіть ми здивовані! 😲 Спробуйте готівку.");
                return View("Checkout", model);
            }
        }

        public IActionResult OrderConfirmation(int orderId)
        {
            var order = _orderRepo.Get(o => o.Id == orderId);
            if (order == null)
            {
                return NotFound();
            }

            // Update order status based on payment type
            // If TransactionId is present, it's a Stripe payment -> Paid
            // If TransactionId is null, it's Cash -> Pending (Очікується)
            if (!string.IsNullOrEmpty(order.TransactionId))
            {
                order.OrderStatus = "Paid";
                _orderRepo.Update(order);
                _db.SaveChanges();
            }

            // Clear cart
            HttpContext.Session.Remove(SessionCartKey);

            return View(order);
        }

        public IActionResult OrderFailure(int orderId)
        {
            // Use _db to include related data
            var order = _db.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.Id == orderId);

            if (order != null)
            {
                var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(SessionCartKey) ?? new List<CartItem>();

                // Restore cart if session was lost (cart is empty)
                if (!cart.Any() && order.OrderDetails.Any())
                {
                    foreach (var detail in order.OrderDetails)
                    {
                        cart.Add(new CartItem
                        {
                            ProductId = detail.ProductId,
                            Name = detail.Product != null ? detail.Product.Name : "Unknown Product",
                            Price = detail.Price,
                            Quantity = detail.Quantity,
                            ImageUrl = detail.Product != null ? detail.Product.ImageUrl : ""
                        });
                    }
                    HttpContext.Session.SetObjectAsJson(SessionCartKey, cart);
                    TempData["Info"] = "Ми відновили ваш кошик, щоб ви могли спробувати ще раз! 🛒";
                }
            }

            return View(order);
        }
        private async Task CheckAndAssignLoyalty(ApplicationUser user)
        {
            // Simple Loyalty Rule:
            // > 1000 UAH -> Assign "Loyalty 5%" (Id 1 assumption or lookup)
            // > 5000 UAH -> Assign "Loyalty 10%" (Id 2 assumption or lookup)
            
            // For MVP, we'll check if "Loyalty Bonus" exists and give it if > 1000 spent
            if (user.TotalSpent > 1000)
            {
                // Check if user already got this promo
                var loyaltyPromo = await _db.Promotions.FirstOrDefaultAsync(p => p.Title == "Loyalty Bonus");
                if (loyaltyPromo == null)
                {
                    // Auto-create if not exists (for demo)
                    loyaltyPromo = new Promotion 
                    { 
                        Title = "Loyalty Bonus", 
                        DiscountPercent = 10, 
                        Description = "Знижка за лояльність (>1000 грн)", 
                        ImageUrl = "https://cdn-icons-png.flaticon.com/512/616/616554.png",
                        PromoCode = "LOYALTY10" // Fixed: Added required PromoCode
                    };
                    _db.Promotions.Add(loyaltyPromo);
                    await _db.SaveChangesAsync();
                }

                // Check if already assigned (Used or Unused)
                bool alreadyAssigned = await _db.UserPromotions.AnyAsync(up => up.ApplicationUserId == user.Id && up.PromotionId == loyaltyPromo.Id);
                
                if (!alreadyAssigned)
                {
                    _db.UserPromotions.Add(new UserPromotion
                    {
                        ApplicationUserId = user.Id,
                        PromotionId = loyaltyPromo.Id,
                        IsUsed = false
                    });
                     // Note: SaveChanges called in main flow, but adding to context is enough here if main flow saves.
                     // Actually main flow calls _db.SaveChanges() before Payment. user update calls UpdateAsync.
                     // We need to save UserPromotions here to be safe.
                     await _db.SaveChangesAsync();
                }
            }
        }
        [HttpPost]
        public async Task<IActionResult> ValidatePromoCode([FromBody] PromoValidationRequest request)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(SessionCartKey) ?? new List<CartItem>();
            if (!cart.Any())
            {
                return Json(new { success = false, message = "Кошик порожній! 🛒" });
            }

            if (string.IsNullOrWhiteSpace(request?.PromoCode))
            {
                return Json(new { success = false, message = "Введіть код!" });
            }

            string code = request.PromoCode.Trim().ToUpper();
            
            // Check manual promo
            var manualPromo = await _db.Promotions
                .FirstOrDefaultAsync(p => p.PromoCode == code && p.IsActive);

            if (manualPromo == null)
            {
                // Funny error messages
                string[] errors = { 
                    "Так просто на клавіатурі наклацав? 😉", 
                    "Цей код з майбутнього? Бо зараз він не працює 🔮", 
                    "Спроба хороша, але ні 🚫",
                    "Хм... Може іншою мовою? 🌍"
                };
                var random = new Random();
                string errorMsg = errors[random.Next(errors.Length)];
                
                return Json(new { success = false, message = errorMsg });
            }

            // Calculate impact
            decimal cartTotal = cart.Sum(c => c.Price * c.Quantity);
            decimal discount = 0;
            string title = manualPromo.Title;

            if (manualPromo.DiscountPercent.HasValue)
            {
                discount = cartTotal * (manualPromo.DiscountPercent.Value / 100m);
            }

            // Check if user has a BETTER auto-promo
            if (User.Identity.IsAuthenticated)
            {
                 var user = await _userManager.GetUserAsync(User);
                 if (user != null)
                 {
                    var autoPromoLink = await _db.UserPromotions
                        .Include(up => up.Promotion)
                        .Where(up => up.ApplicationUserId == user.Id && !up.IsUsed && up.Promotion.IsActive)
                        .OrderByDescending(up => up.Promotion.DiscountPercent)
                        .FirstOrDefaultAsync();

                    if (autoPromoLink != null && autoPromoLink.Promotion.DiscountPercent > manualPromo.DiscountPercent)
                    {
                         return Json(new { 
                            success = true, 
                            message = $"Ваша персональна знижка '{autoPromoLink.Promotion.Title}' (-{autoPromoLink.Promotion.DiscountPercent}%) вигідніша за цей код!",
                            isBetterAutoExists = true,
                            discountAmount = cartTotal * (autoPromoLink.Promotion.DiscountPercent.Value / 100m)
                        });
                    }
                 }
            }

            return Json(new { 
                success = true, 
                message = $"Промокод '{manualPromo.PromoCode}' застосовано! 🎉",
                discountAmount = discount,
                newTotal = cartTotal - discount,
                promoTitle = title,
                percent = manualPromo.DiscountPercent
            });
        }

        public class PromoValidationRequest
        {
            public string PromoCode { get; set; }
        }
    }
}

