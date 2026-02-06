// JavaScript для покращення UX форми оформлення замовлення

document.addEventListener('DOMContentLoaded', function () {
    const phoneInput = document.querySelector('input[name="CustomerPhone"]');
    const nameInput = document.querySelector('input[name="CustomerName"]');
    const addressInput = document.querySelector('textarea[name="CustomerAddress"]');
    const form = document.querySelector('.checkout-form form');

    // Блокування вводу цифр в поле імені
    if (nameInput) {
        nameInput.addEventListener('input', function (e) {
            // Замінюємо будь-які цифри на порожній рядок
            if (/\d/.test(this.value)) {
                this.value = this.value.replace(/\d/g, '');

                // Можна додати візуальну індикацію
                this.classList.add('is-invalid');
                setTimeout(() => {
                    this.classList.remove('is-invalid');
                }, 1000);
            }
        });

        nameInput.addEventListener('keydown', function (e) {
            // Блокуємо натискання клавіш з цифрами (верхній ряд і numpad)
            if ((e.key >= '0' && e.key <= '9') && e.key !== ' ') {
                e.preventDefault();
            }
        });
    }

    // Форматування номера телефону
    if (phoneInput) {
        phoneInput.addEventListener('input', function (e) {
            let value = e.target.value.replace(/\D/g, ''); // Видаляємо всі нецифрові символи

            if (value.startsWith('380')) {
                value = '+' + value;
            } else if (value.length > 0 && !value.startsWith('+')) {
                value = '+380' + value;
            }

            e.target.value = value;
        });
    }

    // Анімація лейблів (Floating Labels)
    const inputs = document.querySelectorAll('.form-control');
    inputs.forEach(input => {
        // Перевірка при завантаженні (якщо є значення, підняти лейбл)
        if (input.value) {
            input.classList.add('has-value');
        }

        input.addEventListener('input', function () {
            if (this.value) {
                this.classList.add('has-value');
            } else {
                this.classList.remove('has-value');
            }
        });
    });
});

