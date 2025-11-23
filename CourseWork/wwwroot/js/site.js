// Add to Cart Animation with AJAX (prevents page reload and screen flicker)
document.addEventListener('DOMContentLoaded', function() {
    const addToCartForms = document.querySelectorAll('.add-to-cart-form');
    
    addToCartForms.forEach(form => {
        form.addEventListener('submit', function(e) {
            e.preventDefault(); // Prevent default form submission
            
            const button = form.querySelector('.add-to-cart-btn');
            const productCard = form.closest('.product-card');
            const productImage = productCard.querySelector('img');
            const floatingCart = document.getElementById('floatingCart');
            const floatingCartCount = document.getElementById('floatingCartCount');
            const navCartBadge = document.querySelector('.navbar .badge');
            
            // Disable button during request
            button.disabled = true;
            const originalText = button.textContent;
            
            // Create flying product animation
            const flyingProduct = productImage.cloneNode(true);
            flyingProduct.style.cssText = `
                position: fixed;
                width: 80px;
                height: 80px;
                object-fit: cover;
                border-radius: 10px;
                z-index: 10000;
                pointer-events: none;
                box-shadow: 0 4px 12px rgba(0,0,0,0.3);
                transition: all 0.8s cubic-bezier(0.4, 0, 0.2, 1);
            `;
            
            const rect = productImage.getBoundingClientRect();
            const cartRect = floatingCart ? floatingCart.getBoundingClientRect() : { left: window.innerWidth - 100, top: window.innerHeight - 100, width: 0, height: 0 };
            
            flyingProduct.style.left = rect.left + 'px';
            flyingProduct.style.top = rect.top + 'px';
            
            document.body.appendChild(flyingProduct);
            
            // Get form data
            const formData = new FormData(form);
            
            // Get anti-forgery token
            const token = form.querySelector('input[name="__RequestVerificationToken"]')?.value;
            if (token) {
                formData.append('__RequestVerificationToken', token);
            }
            
            // Make AJAX request
            fetch(form.action, {
                method: 'POST',
                body: formData,
                credentials: 'same-origin',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'Accept': 'application/json'
                }
            })
            .then(response => {
                if (response.ok) {
                    return response.json().then(data => {
                        // Animate to cart
                        setTimeout(() => {
                            flyingProduct.style.left = cartRect.left + (cartRect.width / 2) - 40 + 'px';
                            flyingProduct.style.top = cartRect.top + (cartRect.height / 2) - 40 + 'px';
                            flyingProduct.style.transform = 'scale(0.3)';
                            flyingProduct.style.opacity = '0.5';
                        }, 10);
                        
                        // Remove element and update UI
                        setTimeout(() => {
                            flyingProduct.remove();
                            
                            // Animate button
                            button.classList.add('added');
                            button.textContent = '✓ Додано!';
                            
                            // Animate floating cart
                            if (floatingCart) {
                                floatingCart.style.transform = 'scale(1.2)';
                                setTimeout(() => {
                                    floatingCart.style.transform = '';
                                }, 300);
                            }
                            
                            // Update cart count from server response
                            if (data.cartCount !== undefined) {
                                if (floatingCartCount) {
                                    floatingCartCount.textContent = data.cartCount;
                                }
                                if (navCartBadge) {
                                    navCartBadge.textContent = data.cartCount;
                                } else {
                                    // If badge doesn't exist, create it
                                    const navLink = document.querySelector('.nav-link[asp-controller="Cart"]');
                                    if (navLink && data.cartCount > 0) {
                                        let badge = navLink.querySelector('.badge');
                                        if (!badge) {
                                            badge = document.createElement('span');
                                            badge.className = 'position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger';
                                            navLink.appendChild(badge);
                                        }
                                        badge.textContent = data.cartCount;
                                    }
                                }
                            }
                            
                            // Reset button after delay
                            setTimeout(() => {
                                button.classList.remove('added');
                                button.textContent = originalText;
                                button.disabled = false;
                            }, 2000);
                        }, 800);
                    }).catch(() => {
                        // If response is not JSON, treat as success anyway
                        setTimeout(() => {
                            flyingProduct.remove();
                            button.disabled = false;
                            button.textContent = originalText;
                        }, 800);
                    });
                } else {
                    throw new Error('Failed to add to cart');
                }
            })
            .catch(error => {
                console.error('Error adding to cart:', error);
                flyingProduct.remove();
                button.disabled = false;
                button.textContent = originalText;
                alert('Помилка при додаванні товару до кошика. Спробуйте ще раз.');
            });
        });
    });
    
    // Update floating cart count on page load
    updateFloatingCart();
});

// Function to update floating cart count
function updateFloatingCart() {
    // This will be called after cart updates
    const floatingCartCount = document.getElementById('floatingCartCount');
    if (floatingCartCount) {
        // Count will be updated by server-side rendering
    }
}

// Smooth scroll for anchor links
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            target.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        }
    });
});

// Navbar scroll effect
window.addEventListener('scroll', function() {
    const navbar = document.querySelector('.navbar');
    if (navbar) {
        if (window.scrollY > 50) {
            navbar.classList.add('scrolled');
        } else {
            navbar.classList.remove('scrolled');
        }
    }
});
