using Microsoft.AspNetCore.Identity;

namespace CourseWork.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "decimal(18,2)")]
        public decimal TotalSpent { get; set; } = 0;
    }
}

