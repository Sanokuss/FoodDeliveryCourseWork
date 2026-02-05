document.addEventListener('DOMContentLoaded', function () {
    const cartTable = document.querySelector('.cart-table');
    const totalAmountEl = document.querySelector('.total-amount strong');
    const cartBadge = document.getElementById('floatingCartCount');
    const MAX_ITEM_QUANTITY = 15;

    // Helper function to show toast with fallback
    function safeToast(message, type, duration) {
        console.log('safeToast called:', message, type);
        if (typeof showToast === 'function') {
            console.log('Using showToast function');
            showToast(message, type, duration);
        } else if (typeof ToastManager !== 'undefined') {
            console.log('Using ToastManager');
            ToastManager.show(message, type, duration);
        } else {
            console.log('Fallback to alert');
            alert(message);
        }
    }

    if (cartTable) {
        cartTable.addEventListener('click', function (e) {
            const target = e.target;
            const btn = target.closest('.btn-cart-action');

            if (!btn) return;

            e.preventDefault();
            const action = btn.dataset.action;
            const productId = btn.dataset.productId;
            const row = btn.closest('tr');

            if (action === 'increase' || action === 'decrease') {
                const input = row.querySelector('.quantity-input');
                let newQuantity = parseInt(input.value);

                if (action === 'increase') {
                    if (newQuantity >= MAX_ITEM_QUANTITY) {
                        safeToast(`Ого, скільки піц! 🍕 Максимум ${MAX_ITEM_QUANTITY} штук!`, 'warning');
                        return;
                    }
                    newQuantity++;
                }

                if (action === 'decrease') {
                    newQuantity--;
                }

                if (newQuantity < 1) return;

                updateQuantity(productId, newQuantity, row, input);
            } else if (action === 'remove') {
                safeToast('Видаляємо товар...', 'info', 2000);
                removeFromCart(productId, row);
            }
        });

        // Also handle manual input change
        cartTable.addEventListener('change', function (e) {
            if (e.target.classList.contains('quantity-input')) {
                const input = e.target;
                const row = input.closest('tr');
                const productId = input.dataset.productId;
                let newQuantity = parseInt(input.value);

                if (newQuantity < 1) {
                    newQuantity = 1;
                    input.value = 1;
                }

                if (newQuantity > MAX_ITEM_QUANTITY) {
                    newQuantity = MAX_ITEM_QUANTITY;
                    input.value = MAX_ITEM_QUANTITY;
                    safeToast(`Максимум ${MAX_ITEM_QUANTITY} штук одного товару! 🍕`, 'warning');
                }

                updateQuantity(productId, newQuantity, row, input);
            }
        });

        function updateQuantity(productId, quantity, row, input) {
            input.disabled = true;
            row.style.opacity = '0.5';

            const formData = new FormData();
            formData.append('productId', productId);
            formData.append('quantity', quantity);
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            if (token) formData.append('__RequestVerificationToken', token);

            fetch('/Cart/UpdateQuantity', {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'Accept': 'application/json'
                }
            })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        input.value = quantity;
                        input.disabled = false;
                        row.style.opacity = '1';

                        const itemTotalCell = row.querySelector('.item-total');
                        if (itemTotalCell) itemTotalCell.innerText = data.itemTotal + ' ₴';

                        if (totalAmountEl) totalAmountEl.innerText = data.cartTotal + ' ₴';

                        if (cartBadge) {
                            cartBadge.innerText = data.cartCount;
                            cartBadge.classList.remove('d-none');
                        }
                    } else {
                        if (data.limitExceeded) {
                            safeToast(data.message, 'warning');
                        } else {
                            safeToast('Кошик збунтувався! 🛒 Спробуйте ще раз', 'error');
                        }
                        input.disabled = false;
                        row.style.opacity = '1';
                    }
                })
                .catch(err => {
                    console.error(err);
                    safeToast('Щось пішло не так... 😅', 'error');
                    input.disabled = false;
                    row.style.opacity = '1';
                });
        }

        function removeFromCart(productId, row) {
            row.style.opacity = '0.2';

            const formData = new FormData();
            formData.append('productId', productId);
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            if (token) formData.append('__RequestVerificationToken', token);

            fetch('/Cart/RemoveFromCart', {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'Accept': 'application/json'
                }
            })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        row.remove();
                        safeToast('Товар видалено з кошика! 🗑️', 'success');

                        if (totalAmountEl) totalAmountEl.innerText = data.cartTotal + ' ₴';

                        if (cartBadge) {
                            cartBadge.innerText = data.cartCount;
                            if (data.cartCount === 0) cartBadge.classList.add('d-none');
                        }

                        if (data.isEmpty) {
                            location.reload();
                        }
                    }
                })
                .catch(err => {
                    console.error(err);
                    safeToast('Не вдалося видалити товар 😕', 'error');
                    row.style.opacity = '1';
                });
        }
    }
});
