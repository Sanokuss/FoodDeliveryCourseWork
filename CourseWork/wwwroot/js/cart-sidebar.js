// Cart Sidebar JavaScript
(function () {
    'use strict';

    const sidebar = document.getElementById('cartSidebar');
    const overlay = document.getElementById('cartSidebarOverlay');
    const openBtn = document.getElementById('openCartSidebar');
    const contentContainer = document.getElementById('cart-sidebar-content');

    // Open cart sidebar
    function openCartSidebar() {
        loadCartContent();
        sidebar.classList.add('active');
        overlay.classList.add('active');
        document.body.style.overflow = 'hidden';
    }

    // Close cart sidebar
    function closeCartSidebar() {
        sidebar.classList.remove('active');
        overlay.classList.remove('active');
        document.body.style.overflow = '';
    }

    // Load cart content via AJAX
    function loadCartContent() {
        fetch('/Cart/GetCartSidebar')
            .then(response => response.text())
            .then(html => {
                contentContainer.innerHTML = html;
            })
            .catch(err => console.error('Error loading cart:', err));
    }

    // Use event delegation for dynamically loaded content
    if (contentContainer) {
        contentContainer.addEventListener('click', function (e) {
            // Close button
            if (e.target.closest('#closeCartSidebar')) {
                closeCartSidebar();
                return;
            }

            // Quantity buttons
            const qtyBtn = e.target.closest('.qty-btn');
            if (qtyBtn) {
                e.preventDefault();
                e.stopPropagation();
                const productId = qtyBtn.dataset.productId;
                const action = qtyBtn.dataset.action;
                const qtyEl = qtyBtn.parentElement.querySelector('.qty-value');
                let qty = parseInt(qtyEl.textContent);

                if (action === 'increase') {
                    qty++;
                    updateQuantity(productId, qty);
                } else if (action === 'decrease') {
                    if (qty > 1) {
                        qty--;
                        updateQuantity(productId, qty);
                    } else {
                        removeItem(productId);
                    }
                }
                return;
            }

            // Remove button
            const removeBtn = e.target.closest('.btn-remove-item');
            if (removeBtn) {
                e.preventDefault();
                e.stopPropagation();
                const productId = removeBtn.dataset.productId;
                removeItem(productId);
                return;
            }
        });
    }

    // Update item quantity
    function updateQuantity(productId, quantity) {
        fetch('/Cart/UpdateQuantity', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: `productId=${productId}&quantity=${quantity}`
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    loadCartContent();
                    updateCartBadge(data.cartCount);
                } else if (data.limitExceeded) {
                    showToast(data.message, 'warning');
                }
            })
            .catch(err => console.error('Update error:', err));
    }

    // Remove item from cart
    function removeItem(productId) {
        fetch('/Cart/RemoveFromCart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: `productId=${productId}`
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    loadCartContent();
                    updateCartBadge(data.cartCount);
                    showToast('Товар видалено з кошика', 'info');
                }
            })
            .catch(err => console.error('Remove error:', err));
    }

    // Update cart badge in header
    function updateCartBadge(count) {
        const badge = document.querySelector('.cart-badge');
        if (badge) {
            if (count > 0) {
                badge.textContent = count;
                badge.style.display = 'flex';
            } else {
                badge.style.display = 'none';
            }
        }
    }

    // Show toast notification
    function showToast(message, type = 'success') {
        if (typeof window.showToast === 'function') {
            window.showToast(message, type);
        } else {
            console.log('Toast:', message);
        }
    }

    // Event listeners
    if (openBtn) {
        openBtn.addEventListener('click', openCartSidebar);
    }

    if (overlay) {
        overlay.addEventListener('click', closeCartSidebar);
    }

    // ESC key to close
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && sidebar && sidebar.classList.contains('active')) {
            closeCartSidebar();
        }
    });

    // Expose functions globally
    window.closeCartSidebar = closeCartSidebar;
    window.refreshCartSidebar = loadCartContent;
    window.openCartSidebar = openCartSidebar;
})();
