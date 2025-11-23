using Microsoft.AspNetCore.Mvc;
using CourseWork.Models;
using CourseWork.Repositories;
using CourseWork.ViewModels;
using CourseWork.Utility;
using CourseWork.Data;
using Stripe.Checkout;
using Stripe;
using ProductModel = CourseWork.Models.Product;
using Microsoft.Extensions.Configuration;

namespace CourseWork.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IRepository<ProductModel> _productRepo;
        private readonly IRepository<OrderDetail> _orderDetailRepo;
        private readonly ApplicationDbContext _db;
        private const string SessionCartKey = "ShoppingCart";

        public OrderController(
            IOrderRepository orderRepo,
            IRepository<ProductModel> productRepo,
            IRepository<OrderDetail> orderDetailRepo,
            ApplicationDbContext db)
        {
            _orderRepo = orderRepo;
            _productRepo = productRepo;
            _orderDetailRepo = orderDetailRepo;
            _db = db;
        }

        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(SessionCartKey) ?? new List<CartItem>();
            if (!cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            return View(new CheckoutViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PlaceOrder(CheckoutViewModel model)
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

            _orderRepo.Add(order);
            _db.SaveChanges();

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

            // Create Stripe Session
            try
            {
                var stripeSecretKey = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Stripe:SecretKey"];
                
                // Check if Stripe key is configured
                if (string.IsNullOrEmpty(stripeSecretKey) || stripeSecretKey.Contains("your_secret_key"))
                {
                    ModelState.AddModelError("", "Сервіс оплати тимчасово недоступний. Будь ласка, зверніться до адміністратора.");
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
                ModelState.AddModelError("", $"Помилка при створенні сесії оплати: {ex.Message}. Будь ласка, спробуйте пізніше або зверніться до підтримки.");
                return View("Checkout", model);
            }
            catch (Exception)
            {
                // Log the error (in production use proper logging)
                ModelState.AddModelError("", "Сталася неочікувана помилка. Будь ласка, спробуйте пізніше.");
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

            // Update order status to Paid
            order.OrderStatus = "Paid";
            _orderRepo.Update(order);
            _db.SaveChanges();

            // Clear cart
            HttpContext.Session.Remove(SessionCartKey);

            return View(order);
        }

        public IActionResult OrderFailure(int orderId)
        {
            var order = _orderRepo.Get(o => o.Id == orderId);
            return View(order);
        }
    }
}

