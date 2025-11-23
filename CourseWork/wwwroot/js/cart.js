// JavaScript для покращення UX кошика

document.addEventListener('DOMContentLoaded', function() {
    // Показуємо повідомлення про успішне додавання
    const successMessage = document.querySelector('.alert-success');
    if (successMessage) {
        setTimeout(() => {
            successMessage.style.transition = 'opacity 0.5s';
            successMessage.style.opacity = '0';
            setTimeout(() => successMessage.remove(), 500);
        }, 3000);
    }

    // Анімація при видаленні товару
    const removeButtons = document.querySelectorAll('form[action*="RemoveFromCart"]');
    removeButtons.forEach(button => {
        button.addEventListener('submit', function(e) {
            const row = this.closest('tr');
            if (row) {
                row.style.transition = 'all 0.3s ease';
                row.style.opacity = '0';
                row.style.transform = 'translateX(-100px)';
            }
        });
    });

    // Плавна зміна кількості
    const quantityInputs = document.querySelectorAll('.quantity-input');
    quantityInputs.forEach(input => {
        input.addEventListener('change', function() {
            this.style.transform = 'scale(1.1)';
            setTimeout(() => {
                this.style.transform = 'scale(1)';
            }, 200);
        });
    });
});

