using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseWork.Models
{
    public class UserPromotion
    {
        public int Id { get; set; }

        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }

        public int PromotionId { get; set; }
        [ForeignKey("PromotionId")]
        public Promotion Promotion { get; set; }

        public bool IsUsed { get; set; } = false;
    }
}
