function checkValidData(dataObject) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(dataObject['email']) || dataObject['email'].trim() === '') {
        throw new Error('El email no es valido.');
    }
    if (dataObject['username'].trim() === '') {
        throw new Error('El nombre de usuario no puede estar vacío.');
    }
    if (dataObject['password'].length < 6) {
        throw new Error('La contraseña debe tener al menos 6 caracteres.');
    }
    if (dataObject['password'] !== dataObject['confirmPassword']) {
        throw new Error('Las contraseñas no coinciden.');
    }
    return;
}

async function CreateUser() {
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
        console.log('Respuesta del servidor:', data);
        if (response.ok) {
            await showSuccessAlert('¡Éxito!', 'Usuario creado correctamente.');

            window.location.href = '/User';
        } else {
            throw new Error('Ah ocurrido un error, vuelve a intertarlo, si el error persiste, contacte a soporte.');
        }
    } catch (error) {
        await showErrorAlert("Error", error.message);
    } finally {
        Swal.close();
    }
}