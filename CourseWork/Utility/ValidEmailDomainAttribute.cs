using System.ComponentModel.DataAnnotations;

namespace CourseWork.Utility
{
    /// <summary>
    /// Validates that an email domain is not a fake/test domain
    /// </summary>
    public class ValidEmailDomainAttribute : ValidationAttribute
    {
        private static readonly string[] InvalidDomains = new[]
        {
            "test", "example", "localhost", "test.com", "example.com", 
            "fake", "fake.com", "invalid", "invalid.com", "temp", "temp.com"
        };

        public ValidEmailDomainAttribute()
        {
            ErrorMessage = "Хмм, a@test? Ви тестувальник? 🧐 Введіть справжній email!";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success; // Let [Required] handle empty values
            }

            var email = value.ToString()!;
            var atIndex = email.IndexOf('@');
            
            if (atIndex < 0 || atIndex >= email.Length - 1)
            {
                return ValidationResult.Success; // Let [EmailAddress] handle invalid format
            }

            var domain = email.Substring(atIndex + 1).ToLowerInvariant();
            
            // Check if domain is in invalid list or has no TLD (like "test" instead of "test.com")
            foreach (var invalidDomain in InvalidDomains)
            {
                if (domain.Equals(invalidDomain, StringComparison.OrdinalIgnoreCase) ||
                    domain.EndsWith("." + invalidDomain, StringComparison.OrdinalIgnoreCase))
                {
                    return new ValidationResult(ErrorMessage);
                }
            }

            // Check if domain has a valid TLD (at least one dot)
            if (!domain.Contains('.'))
            {
                return new ValidationResult("Хмм, цей домен виглядає підозріло... 🤔 Перевірте email!");
            }

            return ValidationResult.Success;
        }
    }
}
