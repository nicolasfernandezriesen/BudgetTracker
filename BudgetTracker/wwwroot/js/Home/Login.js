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

        const response = await fetch('/Home/Login', {
            method: 'POST',
            body: formData
        });
        const responseData = await response.json();

        if (response.ok) {
            Swal.close();

            await showSuccessAlert('Verificado', responseData.message);

            window.location.href = '/User';
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