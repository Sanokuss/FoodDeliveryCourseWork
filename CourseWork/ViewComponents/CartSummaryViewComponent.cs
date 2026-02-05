using Microsoft.AspNetCore.Mvc;
using CourseWork.Models;
using CourseWork.Utility;
using CourseWork.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace CourseWork.ViewComponents
{
    public class CartSummaryViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("ShoppingCart");
            var count = cart != null ? cart.Sum(c => c.Quantity) : 0;
            return View(count);
        }
    }
}
