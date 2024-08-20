﻿async function CreateUser() {
    const form = document.getElementById('createUserForm');
    const formData = new FormData(form);

    const loadingSwal = showLoadingAlert('Creando usuario');

    await new Promise(resolve => setTimeout(resolve, 1000)); 

    try {
        const response = await fetch('/Home/Create', {
            method: 'POST',
            body: formData
        });

        if (response.ok) {
            await showSuccessAlert('¡Éxito!', 'Usuario creado correctamente.');

            window.location.href = '/User';
        } else {
            throw new Error('Hubo un problema a la hora de crear el usuario. Vuelve a intentarlo.');
        }
    } catch (error) {
        await showErrorAlert("Error", error.message);
    } finally {
        Swal.close();
    }
}