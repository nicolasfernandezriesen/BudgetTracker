function checkEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    var isValid = false

    if (!emailRegex.test(email) || email.trim() === '') {
        isValid = false;
    } else {
        isValid = true;
    }
    return isValid;
}

async function forgotPassword() {
    const email = document.querySelector('input[id="email"]').value;

    if (!checkEmail(email)) {
        await showErrorAlert('Error', 'Por favor, ingresa un email válido.');
        return;
    }

    const isConfirmed = await showConfirmationAlert('Recuperar contraseña', `Se enviará un correo de recuperación a tu email: ${email}. Deseas continuar?`);
    if (!isConfirmed.isConfirmed) {
        return;
    }

    const form = document.getElementById('forgotPasswordForm');
    const formData = new FormData(form);

    try {
        showLoadingAlert('Enviando correo de recuperación');

        const response = await fetch('/Home/ForgotPassword', {
            method: 'POST',
            body: formData
        });
        const data = await response.json();

        if (response.ok) {
            Swal.close();
            await showSuccessAlert('¡Éxito!', data.message);

            window.location.href = '/Home/Index';
        } else {
            throw new Error(data.message);
        }
    } catch (error) {
        Swal.close();
        await showErrorAlert("Error", error.message);
    } finally {
        Swal.close();
    }
}