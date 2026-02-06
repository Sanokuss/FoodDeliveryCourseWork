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

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    model.CustomerName = user.FullName ?? "";
                    model.CustomerPhone = user.PhoneNumber ?? "";
                    model.CustomerAddress = user.Address ?? "";
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
                TotalAmount = cart.Sum(c => c.Price * c.Quantity),
                OrderStatus = "Pending",
                OrderDate = DateTime.Now
            };

            // Link to User if authenticated
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    order.ApplicationUserId = user.Id;

                    // Optional: Update user profile if empty
                    bool userUpdated = false;
                    if (string.IsNullOrEmpty(user.FullName)) { user.FullName = model.CustomerName; userUpdated = true; }
                    if (string.IsNullOrEmpty(user.PhoneNumber)) { user.PhoneNumber = model.CustomerPhone; userUpdated = true; }
                    if (string.IsNullOrEmpty(user.Address)) { user.Address = model.CustomerAddress; userUpdated = true; }
                    
                    if (userUpdated)
                    {
                        await _userManager.UpdateAsync(user);
                    }
                }
            }

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
    }
}

