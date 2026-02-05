using System.ComponentModel.DataAnnotations;

namespace CourseWork.Models
{
    public class Promotion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Назва акції обов'язкова")]
        public string Title { get; set; }

        public string Description { get; set; }

        [Display(Name = "URL Зображення")]
        public string ImageUrl { get; set; }

        [Display(Name = "Знижка (%)")]
        public int? DiscountPercent { get; set; }

        public string PromoCode { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
