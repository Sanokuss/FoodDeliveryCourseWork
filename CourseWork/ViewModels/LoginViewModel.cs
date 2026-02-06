using System.ComponentModel.DataAnnotations;

namespace CourseWork.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Куди ж без email? Введіть, будь ласка! 📬")]
        [EmailAddress(ErrorMessage = "Це email чи код до сейфа? Перевірте формат! 🔑")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль забули? Бувало... Але введіть щось! 🧠")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Запам'ятати мене")]
        public bool RememberMe { get; set; } = true;
        
        [Display(Name = "Повернутись до")]
        public string? ReturnUrl { get; set; } = string.Empty;
    }
}
