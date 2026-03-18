function checkPassword(dataObject) {
    const digitRegex = /\d/;
    const errors = [];

    if (!dataObject['NewPassword'] || !dataObject['ConfirmPassword']) {
        errors.push('Todos los campos son obligatorios.');
        throw new Error(errors.join('\n'));
    }

    if (dataObject['NewPassword'].length < 6) {
        errors.push('La contraseña debe tener al menos 6 caracteres.');
    }

    if (!digitRegex.test(dataObject['NewPassword'])) {
        errors.push('La contraseña debe contener al menos un número.');
    }

    if (dataObject['NewPassword'] !== dataObject['ConfirmPassword']) {
        errors.push('Las contraseñas no coinciden.');
    }

    if (errors.length > 0) {
        throw new Error(errors.join('\n'));
    }

    return;
}

async function ResetPassword() {
    const isConfirmed = await showConfirmationAlert('Cambio de contraseña', 'Estas seguro que queres cambiar a esa contraseña?');

    if (!isConfirmed.isConfirmed) {
        return;
    }

    const tokenFront = document.querySelector('input[name="__RequestVerificationToken"]').value;
    const newPassword = document.getElementById('newPassword').value;
    const confirmPassword = document.getElementById('confirmPassword').value;

    const dataObject = {
        Email: _CONFIG_RESET.email,
        Token: _CONFIG_RESET.token,
        NewPassword: newPassword,
        ConfirmPassword: confirmPassword
    };

    try {
        checkPassword(dataObject);

        const loadingSwal = showLoadingAlert('Cambiando contraseña');

        const response = await fetch('/User/ResetPassword', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': tokenFront
            },
            body: JSON.stringify(dataObject)
        });

        const data = await response.json();

        if (response.ok) {
            Swal.close();
            await showSuccessAlert('¡Éxito!', data.message);

            window.location.href = '/User/Index';
        } else {
            throw new Error(data.message);
        }
    } catch (error) {
        Swal.close();
        await showErrorAlert('Error', error.message);
    } finally {
        Swal.close();
    }
}