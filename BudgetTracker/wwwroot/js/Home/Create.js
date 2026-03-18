function checkValidData(dataObject) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    const digitRegex = /\d/;
    const errors = [];

    if (!emailRegex.test(dataObject['Email']) || dataObject['Email'].trim() === '') {
        errors.push('El email no es valido.');
    }

    if (dataObject['UserName'].trim() === '') {
        errors.push('El nombre de usuario no puede estar vacío.');
    }

    if (dataObject['Password'].length < 6) {
        errors.push('La contraseña debe tener al menos 6 caracteres.');
    }

    if (!digitRegex.test(dataObject['Password'])) {
        errors.push('La contraseña debe contener al menos un número.');
    }

    if (dataObject['Password'] !== dataObject['ConfirmPassword']) {
        errors.push('Las contraseñas no coinciden.');
    }

    if (errors.length > 0) {
        throw new Error(errors.join('\n'));
    }

    return;
}

async function CreateUser() {
    const isConfirmed = await showConfirmationAlert('Creacion de usuario', 'Estas seguro que queres crear este usuario?');

    if (!isConfirmed.isConfirmed) {
        return;
    }

    const form = document.getElementById('createUserForm');
    const formData = new FormData(form);

    const dataObject = {};
    formData.forEach((value, key) => {
        dataObject[key] = value;
    });

    try {
        checkValidData(dataObject);

        const loadingSwal = showLoadingAlert('Creando usuario');

        const response = await fetch('/Home/Create', {
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