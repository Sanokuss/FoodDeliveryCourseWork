/**
 * Toast Notification System for BodanFood
 * Beautiful, animated toast messages with humor! 🍕
 */

const ToastManager = (function () {
    let container = null;

    // Icons for different toast types
    const icons = {
        success: `<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path><polyline points="22 4 12 14.01 9 11.01"></polyline></svg>`,
        error: `<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"></circle><line x1="15" y1="9" x2="9" y2="15"></line><line x1="9" y1="9" x2="15" y2="15"></line></svg>`,
        warning: `<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path><line x1="12" y1="9" x2="12" y2="13"></line><line x1="12" y1="17" x2="12.01" y2="17"></line></svg>`,
        info: `<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"></circle><line x1="12" y1="16" x2="12" y2="12"></line><line x1="12" y1="8" x2="12.01" y2="8"></line></svg>`
    };

    function init() {
        if (!container) {
            container = document.createElement('div');
            container.id = 'toast-container';
            container.className = 'toast-container';
            container.style.cssText = `
                position: fixed !important;
                top: 80px !important;
                right: 16px !important;
                z-index: 99999 !important;
                display: flex !important;
                flex-direction: column !important;
                max-width: 400px !important;
                width: calc(100% - 32px) !important;
                pointer-events: none !important;
            `;
            document.body.appendChild(container);
            console.log('Toast container created:', container);
        }
    }

    function show(message, type = 'info', duration = 4000) {
        init();
        console.log('Toast container:', container);

        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;

        // Add inline styles to ensure visibility
        toast.style.cssText = `
            display: flex !important;
            align-items: flex-start;
            gap: 12px;
            padding: 16px;
            background: #fff;
            border-radius: 16px;
            box-shadow: 0 10px 40px rgba(0, 0, 0, 0.3);
            border: 1px solid #ddd;
            position: relative;
            overflow: hidden;
            margin-bottom: 12px;
            opacity: 1 !important;
            transform: translateX(0) !important;
            pointer-events: auto !important;
            border-left: 4px solid ${type === 'error' ? '#ff7ca3' : type === 'warning' ? '#ffc107' : type === 'success' ? '#50d1aa' : '#9288e0'};
        `;

        toast.innerHTML = `
            <div class="toast-icon" style="flex-shrink: 0; display: flex; align-items: center; justify-content: center; width: 40px; height: 40px; border-radius: 12px; background: ${type === 'error' ? 'rgba(255,124,163,0.15)' : type === 'warning' ? 'rgba(255,193,7,0.15)' : type === 'success' ? 'rgba(80,209,170,0.15)' : 'rgba(146,136,224,0.15)'}; color: ${type === 'error' ? '#ff7ca3' : type === 'warning' ? '#ffc107' : type === 'success' ? '#50d1aa' : '#9288e0'};">${icons[type] || icons.info}</div>
            <div class="toast-content" style="flex: 1; min-width: 0;">
                <p class="toast-message" style="margin: 0; font-size: 14px; font-weight: 500; color: #333; line-height: 1.5;">${message}</p>
            </div>
            <button class="toast-close" aria-label="Закрити" style="flex-shrink: 0; background: transparent; border: none; color: #999; cursor: pointer; padding: 4px; border-radius: 8px;">
                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
            </button>
        `;

        // Close button handler
        toast.querySelector('.toast-close').addEventListener('click', () => {
            removeToast(toast);
        });

        container.appendChild(toast);
        console.log('Toast added to container:', toast);

        // Auto-remove after duration
        if (duration > 0) {
            setTimeout(() => {
                removeToast(toast);
            }, duration);
        }

        return toast;
    }

    function removeToast(toast) {
        if (!toast || toast.classList.contains('toast-hide')) return;

        toast.classList.remove('toast-show');
        toast.classList.add('toast-hide');

        setTimeout(() => {
            if (toast.parentNode) {
                toast.parentNode.removeChild(toast);
            }
        }, 300);
    }

    function success(message, duration = 4000) {
        return show(message, 'success', duration);
    }

    function error(message, duration = 5000) {
        return show(message, 'error', duration);
    }

    function warning(message, duration = 4500) {
        return show(message, 'warning', duration);
    }

    function info(message, duration = 4000) {
        return show(message, 'info', duration);
    }

    return {
        show,
        success,
        error,
        warning,
        info
    };
})();

// Global shortcut functions
function showToast(message, type = 'info', duration = 4000) {
    return ToastManager.show(message, type, duration);
}

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { ToastManager, showToast };
}
