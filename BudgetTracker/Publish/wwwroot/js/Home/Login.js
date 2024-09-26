function checkValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

async function LoginUser() {
    const form = document.getElementById('loginUserForm');
    const formData = new FormData(form);

    const loadingSwal = showLoadingAlert('Iniciando sesion');

    await new Promise(resolve => setTimeout(resolve, 1000));

    const dataObject = {};
    formData.forEach((value, key) => {
        dataObject[key] = value;
    });

    try {
        if (!checkValidEmail(dataObject['UserEmail'])) {
            throw new Error('El email no es valido.');
        }

        const response = await fetch('/Home/Login', {
            method: 'POST',
            body: formData
        });

        if (response.ok) {
            showSuccessAlert('Verificado', 'Te has logeado correctamente.');

            await new Promise(resolve => setTimeout(resolve, 1000));
            Swal.close();

            window.location.href = '/User';
        } else {
            throw new Error('Credenciales no validas.');
        }
    } catch (error) {
        await showErrorAlert("Error", error.message);
    } finally {
        Swal.close();
    }
}