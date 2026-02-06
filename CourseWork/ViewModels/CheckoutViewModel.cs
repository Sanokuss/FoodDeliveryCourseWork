using System.ComponentModel.DataAnnotations;
using CourseWork.Utility;

namespace CourseWork.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Кур'єр питатиме: \"А для кого це?\" Введіть ім'я! 📦")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Ім'я занадто коротке чи довге! Від 2 до 100 символів 📏")]
        [RegularExpression(@"^[a-zA-Zа-яА-ЯіІїЇєЄґҐ\s\-']+$", ErrorMessage = "В імені можуть бути тільки літери. Цифри залиште для номера телефону! ✍️")]
        [Display(Name = "Ім'я")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Телефон потрібен! Як інакше кур'єр вам зателефонує? 📞")]
        [Display(Name = "Телефон")]
        [UkrainianPhone(ErrorMessage = "Телефон якийсь дивний... +380XXXXXXXXX, будь ласка! 📱")]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Куди везти смаколики? Вкажіть адресу! 🏠")]
        [StringLength(200, MinimumLength = 10, ErrorMessage = "Адреса занадто коротка — кур'єр заблукає! 🗺️")]
        [Display(Name = "Адреса доставки")]
        public string CustomerAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Будь ласка, оберіть спосіб оплати! 💸")]
        [Display(Name = "Спосіб оплати")]
        public string PaymentMethod { get; set; } = "Card"; // Default to Card

        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public string? AppliedPromoCode { get; set; }

        [Display(Name = "Промокод")]
        public string? ManualPromoCode { get; set; }
    }
}
