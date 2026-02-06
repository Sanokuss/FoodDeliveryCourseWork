document.addEventListener('DOMContentLoaded', function () {
    // --- Sidebar & Mobile Menu Logic ---
    const sidebar = document.querySelector('.sidebar');
    const mobileToggle = document.querySelector('#mobileMenuToggle');
    const mainContent = document.querySelector('.main-content');
    const body = document.body;

    if (mobileToggle) {
        mobileToggle.addEventListener('click', function () {
            sidebar.classList.toggle('active');
        });
    }

    // Close sidebar when clicking outside on mobile
    document.addEventListener('click', function (event) {
        if (window.innerWidth <= 768 && sidebar && mobileToggle) {
            if (!sidebar.contains(event.target) && !mobileToggle.contains(event.target) && sidebar.classList.contains('active')) {
                sidebar.classList.remove('active');
            }
        }
    });

    // --- Theme Toggle Logic ---
    const themeToggle = document.getElementById('themeToggle');
    const sunIcon = document.querySelector('.sun-icon');
    const moonIcon = document.querySelector('.moon-icon');

    // Check saved theme or default to light
    const savedTheme = localStorage.getItem('theme') || 'light';
    applyTheme(savedTheme);

    if (themeToggle) {
        themeToggle.addEventListener('click', function () {
            // Check if dark class/attribute is present to decide next state
            const isDark = body.getAttribute('data-theme') === 'dark';
            const newTheme = isDark ? 'light' : 'dark';

            applyTheme(newTheme);
            localStorage.setItem('theme', newTheme);
        });
    }

    function applyTheme(theme) {
        if (theme === 'dark') {
            body.setAttribute('data-theme', 'dark');
            // CSS handles icon visibility/animation now
        } else {
            body.removeAttribute('data-theme');
        }
    }

    // --- Hero Typewriter Effect (BodanFood) ---
    // --- Hero Typewriter Effect (BodanFood) ---
    // Replaced by SVG Ink Animation in View


    // --- Typewriter Effect for Search Placeholder ---
    const searchInput = document.querySelector('input[name="searchTerm"]');
    if (searchInput) {
        const placeholderText = "Що бажаєте з'їсти?";
        const phrases = [
            "Що бажаєте з'їсти?",
            "Не їж мене, з'їж піцу",
            "Знайди своє щастя (суші)",
            "Ковбаски... багато ковбасок",
            "Напиши 'Margarita' — я знайду",
            "Введи 'сир' і посміхнісь"
        ];
        let phraseIndex = 0;
        let charIndex = 0;
        let isDeleting = false;
        let typeSpeed = 100;

        function type() {
            const currentPhrase = phrases[phraseIndex];

            if (isDeleting) {
                searchInput.placeholder = currentPhrase.substring(0, charIndex - 1);
                charIndex--;
                typeSpeed = 50;
            } else {
                searchInput.placeholder = currentPhrase.substring(0, charIndex + 1);
                charIndex++;
                typeSpeed = 100;
            }

            if (!isDeleting && charIndex === currentPhrase.length) {
                isDeleting = true;
                typeSpeed = 2000; // Pause at end
            } else if (isDeleting && charIndex === 0) {
                isDeleting = false;
                phraseIndex = (phraseIndex + 1) % phrases.length;
                typeSpeed = 500; // Pause before new word
            }

            setTimeout(type, typeSpeed);
        }

        // Start typing
        setTimeout(type, 1000);
    }

    // --- Add to Cart Animation ---
    // Use event delegation for better support of dynamic content
    document.body.addEventListener('submit', function (e) {
        if (e.target.matches('.add-to-cart-form')) {
            e.preventDefault();
            const form = e.target;

            // Selector support for both old and new designs
            const button = form.querySelector('.add-to-cart-btn, .btn-add');
            const productCard = form.closest('.glass-card, .card, .food-card');

            if (!button || !productCard) return;

            const productImage = productCard.querySelector('img');

            // Target the Cart icon (Sidebar or TopNav)
            const cartLink = document.querySelector('#floatingCart, .nav-cart-btn, a[href*="Cart"]');

            button.disabled = true;
            const originalText = button.innerHTML;

            // Flying Image Animation
            if (productImage && cartLink) {
                const flyingProduct = productImage.cloneNode(true);
                flyingProduct.style.cssText = `
                    position: fixed;
                    z-index: 10000;
                    pointer-events: none;
                    width: 100px;
                    height: 100px;
                    object-fit: cover;
                    border-radius: 50%;
                    transition: all 0.8s cubic-bezier(0.175, 0.885, 0.32, 1.275);
                    opacity: 0.8;
                `;

                const rect = productImage.getBoundingClientRect();
                const cartRect = cartLink.getBoundingClientRect();

                // Start position
                flyingProduct.style.left = rect.left + 'px';
                flyingProduct.style.top = rect.top + 'px';
                flyingProduct.style.width = rect.width + 'px';
                flyingProduct.style.height = rect.height + 'px';

                document.body.appendChild(flyingProduct);

                // End position (Fly to cart)
                requestAnimationFrame(() => {
                    flyingProduct.style.left = (cartRect.left + cartRect.width / 4) + 'px';
                    flyingProduct.style.top = (cartRect.top + cartRect.height / 4) + 'px';
                    flyingProduct.style.width = '20px';
                    flyingProduct.style.height = '20px';
                    flyingProduct.style.opacity = '0';
                    flyingProduct.style.transform = 'scale(0.5)';
                });

                setTimeout(() => flyingProduct.remove(), 800);
            }

            // AJAX Request
            const formData = new FormData(form);
            const token = form.querySelector('input[name="__RequestVerificationToken"]')?.value;
            if (token) formData.append('__RequestVerificationToken', token);

            fetch(form.action, {
                method: 'POST',
                body: formData,
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            })
                .then(res => {
                    if (!res.ok) throw new Error('Network response was not ok');
                    return res.json();
                })
                .then(data => {
                    setTimeout(() => {
                        button.classList.add('added');
                        // Success visual feedback
                        button.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><polyline points="20 6 9 17 4 12"></polyline></svg>';

                        // Update Cart Count Badge
                        const badge = document.getElementById('floatingCartCount');
                        if (badge && data.cartCount !== undefined) {
                            badge.innerText = data.cartCount;
                            badge.classList.remove('d-none');
                            // Small "pop" animation for badge
                            badge.style.transform = 'scale(1.5)';
                            setTimeout(() => badge.style.transform = 'scale(1)', 200);
                        }

                        setTimeout(() => {
                            button.classList.remove('added');
                            button.innerHTML = originalText;
                            button.disabled = false;
                        }, 2000);
                    }, 600); // Wait for flying animation
                })
                .catch(err => {
                    console.error('Error:', err);
                    button.disabled = false; // Re-enable button on error
                    button.innerHTML = originalText;
                    // Optional: Show error toast here
                });
        }
    });

    // --- Page Transition Logic (Fade Out) ---
    document.addEventListener('click', function (e) {
        const link = e.target.closest('a');

        // Conditions to ignore the transition:
        // 1. Not a link
        // 2. Open in new tab
        // 3. Anchor link to same page
        // 4. Specific actions like "Add to Cart" or "Delete" (often forms, but checking just in case)
        // 5. JavaScript links
        // 6. Category cards (handled by AJAX)
        if (!link ||
            link.target === '_blank' ||
            link.getAttribute('href').startsWith('#') ||
            link.getAttribute('href').startsWith('javascript') ||
            link.classList.contains('no-transition') ||
            link.classList.contains('nav-cart-btn') ||
            link.classList.contains('category-card') ||
            e.ctrlKey || e.metaKey // Ctrl/Cmd+Click
        ) {
            return;
        }

        const href = link.getAttribute('href');

        if (href && href !== window.location.href) {
            e.preventDefault();
            const mainContent = document.querySelector('.page-transition');

            if (mainContent) {
                mainContent.classList.add('fade-out');

                // Wait for animation, then navigate
                setTimeout(() => {
                    window.location.href = href;
                }, 300); // Match CSS transition time
            } else {
                window.location.href = href;
            }
        }
    });

    // --- AJAX Category Filtering ---
    const categoryLinks = document.querySelectorAll('.category-card');
    const productsContainer = document.getElementById('products-container');

    if (categoryLinks.length > 0 && productsContainer) {
        categoryLinks.forEach(link => {
            link.addEventListener('click', function (e) {
                e.preventDefault();

                // Get URL and Category ID
                const url = this.getAttribute('href');
                const categoryId = this.dataset.categoryId;

                // Update Active State
                categoryLinks.forEach(l => l.classList.remove('active'));
                this.classList.add('active');

                // Clear Search Input
                const searchInput = document.querySelector('input[name="searchTerm"]');
                if (searchInput) {
                    searchInput.value = '';
                }

                // Update Sort Form Category ID
                const sortCategoryInput = document.querySelector('input[name="categoryId"]');
                if (sortCategoryInput) {
                    sortCategoryInput.value = categoryId;
                }

                // Reset Sort to Default
                const sortSelect = document.querySelector('select[name="sortOrder"]');
                if (sortSelect) {
                    sortSelect.value = "";
                }

                // Add loading state opacity
                productsContainer.style.opacity = '0.5';
                productsContainer.style.transition = 'opacity 0.2s';

                // Fetch new products
                fetch(url, {
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                })
                    .then(response => {
                        if (!response.ok) throw new Error('Network response was not ok');
                        return response.text();
                    })
                    .then(html => {
                        // Update content
                        productsContainer.innerHTML = html;
                        productsContainer.style.opacity = '1';

                        // Update Browser URL without reloading
                        // Remove hash from URL if present for cleaner URL, or keep it if desired
                        const cleanUrl = url.split('#')[0];
                        window.history.pushState({ categoryId: categoryId }, "", cleanUrl);
                    })
                    .catch(error => {
                        console.error('Error fetching products:', error);
                        productsContainer.style.opacity = '1';
                        // Fallback to normal navigation if fetch fails
                        window.location.href = url;
                    });
            });
        });

        // Handle Back/Forward buttons
        window.addEventListener('popstate', function (e) {
            window.location.reload();
            // Ideally we would fetch content again based on e.state or location.href
            // But reload is a safe fallback for now
        });
    }

});


// --- Mobile Drawer Menu ---
const mobileMenuBtn = document.getElementById('mobileMenuBtn');
const mobileDrawer = document.getElementById('mobileDrawer');
const mobileDrawerClose = document.getElementById('mobileDrawerClose');
const mobileDrawerOverlay = document.getElementById('mobileDrawerOverlay');

function openMobileDrawer() {
    if (mobileDrawer) mobileDrawer.classList.add('open');
    if (mobileDrawerOverlay) mobileDrawerOverlay.classList.add('open');
    document.body.style.overflow = 'hidden';
}

function closeMobileDrawer() {
    if (mobileDrawer) mobileDrawer.classList.remove('open');
    if (mobileDrawerOverlay) mobileDrawerOverlay.classList.remove('open');
    document.body.style.overflow = '';
}

if (mobileMenuBtn) {
    mobileMenuBtn.addEventListener('click', openMobileDrawer);
}

if (mobileDrawerClose) {
    mobileDrawerClose.addEventListener('click', closeMobileDrawer);
}

if (mobileDrawerOverlay) {
    mobileDrawerOverlay.addEventListener('click', closeMobileDrawer);
}

// Close drawer on escape key
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') closeMobileDrawer();
});

// Close drawer on resize to desktop
window.addEventListener('resize', function () {
    if (window.innerWidth > 768) closeMobileDrawer();
});
