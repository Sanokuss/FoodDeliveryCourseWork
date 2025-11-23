// JavaScript для покращення UX форми оформлення замовлення

document.addEventListener('DOMContentLoaded', function() {
    const phoneInput = document.querySelector('input[name="CustomerPhone"]');
    const nameInput = document.querySelector('input[name="CustomerName"]');
    const addressInput = document.querySelector('textarea[name="CustomerAddress"]');
    const form = document.querySelector('.checkout-form form');

    // Форматування номера телефону
    if (phoneInput) {
        phoneInput.addEventListener('input', function(e) {
            let value = e.target.value.replace(/\D/g, ''); // Видаляємо всі нецифрові символи
            
            if (value.startsWith('380')) {
                value = '+' + value;
            } else if (value.startsWith('0')) {
                // Залишаємо як є
            } else if (value.length > 0 && !value.startsWith('+')) {
                value = '+380' + value;
            }
            
            e.target.value = value;
        });

        phoneInput.addEventListener('blur', function(e) {
            let value = e.target.value.replace(/\D/g, '');
            if (value.startsWith('380') && !e.target.value.startsWith('+')) {
                e.target.value = '+' + value;
            }
        });
    }

    // Валідація форми перед відправкою
    if (form) {
        form.addEventListener('submit', function(e) {
            let isValid = true;
            const errors = [];

            // Валідація імені
            if (!nameInput.value.trim() || nameInput.value.trim().length < 2) {
                isValid = false;
                errors.push('Ім\'я повинно містити мінімум 2 символи');
            }

            // Валідація телефону
            const phoneRegex = /^(\+380|380|0)?[0-9]{9}$/;
            const phoneValue = phoneInput.value.replace(/\D/g, '');
            if (!phoneRegex.test(phoneValue) || phoneValue.length < 9) {
                isValid = false;
                errors.push('Невірний формат телефону');
            }

            // Валідація адреси
            if (!addressInput.value.trim() || addressInput.value.trim().length < 10) {
                isValid = false;
                errors.push('Адреса повинна містити мінімум 10 символів');
            }

            if (!isValid) {
                e.preventDefault();
                // Показуємо помилки (якщо є контейнер для помилок)
                const errorContainer = document.querySelector('.alert-danger');
                if (errorContainer) {
                    errorContainer.innerHTML = '<ul class="mb-0">' + errors.map(err => '<li>' + err + '</li>').join('') + '</ul>';
                    errorContainer.style.display = 'block';
                }
                return false;
            }
        });
    }

    // Анімація при фокусі на полях
    const inputs = document.querySelectorAll('.form-control');
    inputs.forEach(input => {
        input.addEventListener('focus', function() {
            this.parentElement.classList.add('focused');
        });
        input.addEventListener('blur', function() {
            if (!this.value) {
                this.parentElement.classList.remove('focused');
            }
        });
    });
});

