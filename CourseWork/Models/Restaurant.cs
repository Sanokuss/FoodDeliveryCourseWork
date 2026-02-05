using System.ComponentModel.DataAnnotations;

namespace CourseWork.Models
{
    public class Restaurant
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Назва ресторану обов'язкова")]
        public string Name { get; set; }

        public string Description { get; set; }

        [Display(Name = "URL Логотипу")]
        public string LogoUrl { get; set; }

        [Required(ErrorMessage = "Адреса обов'язкова")]
        public string Address { get; set; }

        public string WorkingHours { get; set; } // e.g. "10:00 - 22:00"
        
        // Navigation property for products
        public List<Product> Products { get; set; } = new List<Product>();
    }
}
