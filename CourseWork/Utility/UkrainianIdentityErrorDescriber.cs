using Microsoft.AspNetCore.Identity;

namespace CourseWork.Utility
{
    /// <summary>
    /// Ukrainian localization for ASP.NET Identity error messages with humor
    /// </summary>
    public class UkrainianIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DuplicateEmail(string email)
        {
            return new IdentityError
            {
                Code = nameof(DuplicateEmail),
                Description = $"Email '{email}' вже зайнятий... Можливо, це ваш клон? 🧬"
            };
        }

        public override IdentityError DuplicateUserName(string userName)
        {
            return new IdentityError
            {
                Code = nameof(DuplicateUserName),
                Description = $"Користувач '{userName}' вже існує! Спробуйте інше ім'я 👤"
            };
        }

        public override IdentityError InvalidEmail(string? email)
        {
            return new IdentityError
            {
                Code = nameof(InvalidEmail),
                Description = "Це не схоже на email... Перевірте ще раз! 📧"
            };
        }

        public override IdentityError InvalidUserName(string? userName)
        {
            return new IdentityError
            {
                Code = nameof(InvalidUserName),
                Description = "Ім'я користувача може містити лише літери та цифри 📝"
            };
        }

        public override IdentityError PasswordTooShort(int length)
        {
            return new IdentityError
            {
                Code = nameof(PasswordTooShort),
                Description = $"Пароль закороткий! Мінімум {length} символів, як {length} шматочків піци 🍕"
            };
        }

        public override IdentityError PasswordRequiresDigit()
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresDigit),
                Description = "Пароль має містити хоча б одну цифру! Без неї нікуди 🔢"
            };
        }

        public override IdentityError PasswordRequiresLower()
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresLower),
                Description = "Додайте хоча б одну маленьку літеру (a-z) до пароля! 🔡"
            };
        }

        public override IdentityError PasswordRequiresUpper()
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresUpper),
                Description = "Пароль має містити хоча б одну велику літеру! CAPS рятує! 🔠"
            };
        }

        public override IdentityError PasswordRequiresNonAlphanumeric()
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresNonAlphanumeric),
                Description = "Додайте спецсимвол (@#$%^&*) для більшої безпеки! 🔐"
            };
        }

        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresUniqueChars),
                Description = $"Пароль має містити щонайменше {uniqueChars} унікальних символів! 🎯"
            };
        }

        public override IdentityError DefaultError()
        {
            return new IdentityError
            {
                Code = nameof(DefaultError),
                Description = "Щось пішло не так... Спробуйте ще раз! 😅"
            };
        }

        public override IdentityError ConcurrencyFailure()
        {
            return new IdentityError
            {
                Code = nameof(ConcurrencyFailure),
                Description = "Хтось вже змінив ці дані. Оновіть сторінку! 🔄"
            };
        }

        public override IdentityError LoginAlreadyAssociated()
        {
            return new IdentityError
            {
                Code = nameof(LoginAlreadyAssociated),
                Description = "Цей акаунт вже прив'язаний до іншого користувача 🔗"
            };
        }

        public override IdentityError InvalidToken()
        {
            return new IdentityError
            {
                Code = nameof(InvalidToken),
                Description = "Недійсний токен! Спробуйте надіслати запит ще раз 🎟️"
            };
        }

        public override IdentityError RecoveryCodeRedemptionFailed()
        {
            return new IdentityError
            {
                Code = nameof(RecoveryCodeRedemptionFailed),
                Description = "Код відновлення недійсний. Перевірте правильність! 🔑"
            };
        }

        public override IdentityError UserAlreadyHasPassword()
        {
            return new IdentityError
            {
                Code = nameof(UserAlreadyHasPassword),
                Description = "У вас вже є пароль! Використайте функцію зміни пароля 🔒"
            };
        }

        public override IdentityError UserNotInRole(string role)
        {
            return new IdentityError
            {
                Code = nameof(UserNotInRole),
                Description = $"Користувач не має ролі '{role}'"
            };
        }

        public override IdentityError UserAlreadyInRole(string role)
        {
            return new IdentityError
            {
                Code = nameof(UserAlreadyInRole),
                Description = $"Користувач вже має роль '{role}'"
            };
        }

        public override IdentityError PasswordMismatch()
        {
            return new IdentityError
            {
                Code = nameof(PasswordMismatch),
                Description = "Невірний пароль! Спробуйте ще раз 🔑"
            };
        }
    }
}
