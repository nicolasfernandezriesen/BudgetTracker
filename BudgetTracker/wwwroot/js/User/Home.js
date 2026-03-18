document.addEventListener("DOMContentLoaded", async function () {
    if (fromEmailConfirmation) {
        await showSuccessAlert('Email confirmado', 'Tu email ha sido confirmado exitosamente. Ahora puedes iniciar sesión con tu cuenta.');
    }
});