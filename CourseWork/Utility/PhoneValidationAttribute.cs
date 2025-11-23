using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CourseWork.Utility
{
    public class UkrainianPhoneAttribute : ValidationAttribute
    {
        private static readonly Regex PhoneRegex = new Regex(@"^(\+380|380|0)[0-9]{9}$", RegexOptions.Compiled);

        public UkrainianPhoneAttribute()
        {
            ErrorMessage = "Невірний формат телефону. Використовуйте формат: +380XXXXXXXXX або 0XXXXXXXXX";
        }

        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return false;
            }

            var phone = value.ToString()!.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
            return PhoneRegex.IsMatch(phone);
        }
    }
}

