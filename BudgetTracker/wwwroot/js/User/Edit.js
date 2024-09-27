document.getElementById('editOptions').addEventListener('change', function () {
    const selectedOption = this.value;
    const sections = document.querySelectorAll('.edit-section');
    const submitBtn = document.getElementById('submitBtn');

    // Hide all sections
    sections.forEach(section => {
        section.style.display = 'none';
    });

    // Show the selected section
    if (selectedOption) {
        document.getElementById(selectedOption + 'Section').style.display = 'block';
        submitBtn.style.display = 'block';
    } else {
        submitBtn.style.display = 'none';
    }
});

function checkValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}
async function sendEditUser() {
    const form = document.getElementById('editUserForm');
    const formData = new FormData(form);

    const loadingSwal = showLoadingAlert();

    const dataObject = {};
    formData.forEach((value, key) => {
        dataObject[key] = value;
    });

    try {
        if (!checkValidEmail(dataObject['UserEmail'])) {
            throw new Error('El email no es valido.');
        }

        const response = await fetch('/User/Edit', {
            method: 'POST',
            body: formData
        });

        if (response.ok) {
            await showSuccessAlert("Guardado", "Los cambios se guardaron con exito.");

            window.location.href = '/User';
        } else {
            throw new Error('Error al guardar los datos. Intentelo de nuevo.');
        }
    } catch (error) {
        await showErrorAlert("Error", error.message);
    } finally {
        Swal.close();
    }
}

function goCreateUserView() {
    window.location.href = '/Home/Create';
}