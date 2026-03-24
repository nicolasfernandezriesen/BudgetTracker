function checkValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

async function LoginUser() {
    const form = document.getElementById('loginUserForm');
    const formData = new FormData(form);

    const dataObject = {};
    formData.forEach((value, key) => {
        dataObject[key] = value;
    });
    try {
        if (dataObject['Email'] === '' || dataObject['Password'] === '') {
            throw new Error('Todos los campos son obligatorios.');
        }
        if (!checkValidEmail(dataObject['Email'])) {
            throw new Error('El email no es valido.');
        }

        const loadingSwal = showLoadingAlert('Iniciando sesion');
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

        const response = await fetch('/Home/Login', {
            method: 'POST',
            body: formData
        });
        const responseData = await response.json();

        if (response.ok) {
            Swal.close();
            await showSuccessAlert('Verificado', responseData.message);

            window.location.href = '/User';
        } else if (responseData.error === 'EmailNotConfirmed') {
            Swal.close();
            const confirmResult = await showConfirmationAlert('Email no confirmado', 'El email no ha sido confirmado. ¿Deseas reenviar el email de confirmación?');

            if (confirmResult.isConfirmed) {
                const loadingResendSwal = showLoadingAlert('Re-enviando email de confirmación');

                const email = dataObject['Email'];

                const formDataResend = new FormData();
                formDataResend.append('email', email);
                formDataResend.append('__RequestVerificationToken', token);

                const responseResend = await fetch('/Home/ReSendConfirmationEmail', {
                    method: 'POST',
                    body: formDataResend
                });
                const responseResendData = await responseResend.json();

                if (responseResend.ok) {
                    Swal.close();
                    await showSuccessAlert('Email re-enviado', responseResendData.message);
                } else {
                    throw new Error(responseResendData.message);
                }
            } else {
                throw new Error(responseData.message);
            }
        } else {
            throw new Error(responseData.message);
        }
    } catch (error) {
        Swal.close();
        await showErrorAlert("Error", error.message);
    } finally {
        Swal.close();
    }
}