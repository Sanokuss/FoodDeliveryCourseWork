using System.ComponentModel.DataAnnotations;

namespace CourseWork.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Як нам вас називати? Інкогніто не приймаємо! 🕵️")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Ваше ім'я занадто коротке! Ви точно не бот? 🤖")]
        [RegularExpression(@"^[a-zA-Zа-яА-ЯіІїЇєЄґҐ\s\-']+$", ErrorMessage = "В імені можуть бути тільки літери. Без цифр і спецсимволів! ✍️")]
        [Display(Name = "Повне ім'я")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email потрібен! Голуби вже не доставляють повідомлення 🕊️")]
        [EmailAddress(ErrorMessage = "Це не схоже на email... Може, забули @? 📧")]
        [CourseWork.Utility.ValidEmailDomain]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(200, MinimumLength = 5, ErrorMessage = "Адреса занадто коротка — кур'єр заблукає! 🗺️")]
        [Display(Name = "Адреса")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Без пароля ніяк! Ваша їжа потребує захисту 🔐")]
        [StringLength(100, ErrorMessage = "Пароль закороткий! Мінімум {2} символів, як {2} шматочків піци 🍕", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Підтвердження пароля")]
        [Compare("Password", ErrorMessage = "Паролі посварились і не співпадають! Помиріть їх 🤝")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
