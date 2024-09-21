function checkValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

async function CreateUser() {
    const form = document.getElementById('createUserForm');
    const formData = new FormData(form);

    const loadingSwal = showLoadingAlert('Creando usuario');

    await new Promise(resolve => setTimeout(resolve, 1000)); 

    const dataObject = {};
    formData.forEach((value, key) => {
        dataObject[key] = value;
    });

    try {
        if (!checkValidEmail(dataObject['email'])) {
            throw new Error('El email no es valido.');
        }

        const response = await fetch('/Home/Create', {
            method: 'POST',
            body: formData
        });

        const data = await response.json();

        if (response.ok) {
            await showSuccessAlert('¡Éxito!', 'Usuario creado correctamente.');

            window.location.href = '/User';
        } else {
            throw new Error(data.message);
        }
    } catch (error) {
        await showErrorAlert("Error", error.message);
    } finally {
        Swal.close();
    }
}