using System.ComponentModel.DataAnnotations;

namespace CourseWork.ViewModels
{
    public class ProfileEditViewModel
    {
        [Required(ErrorMessage = "Як вас звати? Нам важливо знати! 😊")]
        [Display(Name = "Повне ім'я")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Адреса доставки")]
        [StringLength(200, ErrorMessage = "Адреса занадто довга, кур'єр втомиться читати 😅")]
        public string? Address { get; set; }

        [Display(Name = "Номер телефону")]
        [Phone(ErrorMessage = "Це не схоже на телефон... 📱")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Фото профілю")]
        public Microsoft.AspNetCore.Http.IFormFile? ProfilePicture { get; set; }
    }
}
