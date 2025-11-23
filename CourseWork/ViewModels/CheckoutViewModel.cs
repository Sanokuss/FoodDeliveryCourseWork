using System.ComponentModel.DataAnnotations;
using CourseWork.Utility;

namespace CourseWork.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Ім'я обов'язкове")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Ім'я повинно містити від 2 до 100 символів")]
        [Display(Name = "Ім'я")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Телефон обов'язковий")]
        [Display(Name = "Телефон")]
        [UkrainianPhone(ErrorMessage = "Невірний формат телефону. Використовуйте формат: +380XXXXXXXXX або 0XXXXXXXXX")]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Адреса обов'язкова")]
        [StringLength(200, MinimumLength = 10, ErrorMessage = "Адреса повинна містити від 10 до 200 символів")]
        [Display(Name = "Адреса доставки")]
        public string CustomerAddress { get; set; } = string.Empty;
    }
}

