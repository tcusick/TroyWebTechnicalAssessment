window.RentalModals = (function () {
    const modalEl = () => document.getElementById('appModal');
    const modalContent = () => document.getElementById('appModalContent');

    function getToken() {
        const input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

    function showModal(html) {
        const content = modalContent();
        if (!content) return;
        content.innerHTML = html;
        const modal = bootstrap.Modal.getOrCreateInstance(modalEl());
        modal.show();
        bindForms(content);
        if (window.jQuery && $.validator) {
            $.validator.unobtrusive.parse(content);
        }
    }

    function bindForms(root) {
        root.querySelectorAll('form[data-modal-form]').forEach(form => {
            form.addEventListener('submit', async (e) => {
                e.preventDefault();
                if (window.jQuery && $.validator) {
                    const $form = $(form);
                    if ($form.data('validator') && !$form.valid()) {
                        return;
                    }
                }

                const formData = new FormData(form);
                const token = getToken();
                const response = await fetch(form.action, {
                    method: 'POST',
                    headers: {
                        'RequestVerificationToken': token
                    },
                    body: formData
                });

                const contentType = response.headers.get('content-type') || '';
                // Success closes the modal; validation errors return the same partial as HTML.
                if (contentType.includes('application/json')) {
                    const data = await response.json();
                    if (data.success) {
                        bootstrap.Modal.getInstance(modalEl())?.hide();
                        if (form.dataset.successReload === 'true') {
                            location.reload();
                        }
                    }
                    return;
                }

                const html = await response.text();
                if (response.ok) {
                    showModal(html); // Re-render modal with field errors
                } else if (response.status === 400) {
                    showModal(html);
                } else {
                    throw new Error('Request failed.');
                }
            });
        });
    }

    function postJson(url, data) {
        const token = getToken();
        const body = new URLSearchParams();
        Object.keys(data).forEach(key => body.append(key, data[key]));
        body.append('__RequestVerificationToken', token);

        return fetch(url, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': token
            },
            body
        }).then(async response => {
            if (!response.ok) {
                const text = await response.text();
                throw new Error(text || 'Request failed.');
            }
            return response.json();
        });
    }

    document.addEventListener('click', (e) => {
        const trigger = e.target.closest('[data-modal-load]');
        if (!trigger) return;
        e.preventDefault();
        fetch(trigger.getAttribute('data-modal-load'))
            .then(r => r.text())
            .then(html => showModal(html));
    });

    return { postJson, showModal };
})();
