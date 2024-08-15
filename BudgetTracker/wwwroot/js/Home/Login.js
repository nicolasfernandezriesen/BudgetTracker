async function LoginUser() {
    const form = document.getElementById('loginUserForm');
    const formData = new FormData(form);

    const loadingSwal = showLoadingAlert('Iniciando sesion');

    await new Promise(resolve => setTimeout(resolve, 1000)); 

    try {
        const response = await fetch('/Home/Login', {
            method: 'POST',
            body: formData
        });

        if (response.ok) {
            await showSuccessAlert('Verificado', 'Te has logeado correctamente.');

            window.location.href = '/Home';
        } else {
            throw new Error('Credenciales no validas.');
        }
    } catch (error) {
        await showErrorAlert("Error", error.message);
    } finally {
        Swal.close();
    }
}