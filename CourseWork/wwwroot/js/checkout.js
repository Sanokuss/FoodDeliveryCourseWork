$(document).ready(function () {
    const $promoInput = $('#ManualPromoCode');
    const $summaryContainer = $('.order-summary');
    let typingTimer;
    const doneTypingInterval = 800; // Wait 800ms after user stops typing

    // Prevent form submission on Enter in promo field
    $promoInput.on('keypress', function (e) {
        if (e.which == 13) {
            e.preventDefault();
            return false;
        }
    });

    $promoInput.on('input', function () {
        clearTimeout(typingTimer);
        const code = $(this).val().trim();

        // Reset styles
        $promoInput.removeClass('is-valid is-invalid');
        $('.promo-feedback').remove();

        if (code.length > 2) {
            typingTimer = setTimeout(validatePromo, doneTypingInterval);
        }
    });

    function validatePromo() {
        const code = $promoInput.val().trim();
        if (!code) return;

        $promoInput.addClass('loading-promo'); // Optional CSS class for spinner

        $.ajax({
            url: '/Order/ValidatePromoCode',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ promoCode: code }),
            success: function (response) {
                $promoInput.removeClass('loading-promo');
                $('.promo-feedback').remove(); // Clear old messages

                if (response.success) {
                    $promoInput.addClass('is-valid').removeClass('is-invalid');

                    // Show success message
                    const $feedback = $(`<div class="valid-feedback promo-feedback d-block animate__animated animate__fadeInUp">
                        ${response.message}
                    </div>`);
                    $promoInput.after($feedback);

                    // Update Summary DOM
                    if (response.discountAmount > 0) {
                        updateOrderSummary(response);
                        // Trigger confetti or highlight
                        $summaryContainer.addClass('highlight-success');
                        setTimeout(() => $summaryContainer.removeClass('highlight-success'), 1000);
                    }
                } else {
                    $promoInput.addClass('is-invalid').removeClass('is-valid');

                    // Show funny error
                    const $feedback = $(`<div class="invalid-feedback promo-feedback d-block animate__animated animate__shakeX">
                        ${response.message}
                    </div>`);
                    $promoInput.after($feedback);
                }
            },
            error: function () {
                $promoInput.removeClass('loading-promo');
            }
        });
    }

    function updateOrderSummary(data) {
        // Construct new summary HTML
        let html = `
            <div class="d-flex justify-content-between text-muted text-decoration-line-through small">
                <span>Сума замовлення:</span>
                <span>${(data.newTotal + data.discountAmount).toFixed(2)} ₴</span>
            </div>
            <div class="d-flex justify-content-between text-success fw-bold my-1 animate__animated animate__pulse">
                <span>Знижка (${data.promoTitle}):</span>
                <span>-${data.discountAmount.toFixed(2)} ₴</span>
            </div>
            ${data.isBetterAutoExists ? `<div class="alert alert-info mt-2 mb-2 p-2 small text-center">${data.message}</div>` : ''}
            <hr class="my-2" />
            <div class="d-flex justify-content-between fs-4 fw-bold" style="color: var(--primary-color);">
                <span>До сплати:</span>
                <span>${data.newTotal.toFixed(2)} ₴</span>
            </div>
        `;

        $summaryContainer.html(html);
    }
});
