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

async function sendEditUser() {
    const form = document.getElementById('editUserForm');
    const formData = new FormData(form);

    const loadingSwal = showLoadingAlert();

    await new Promise(resolve => setTimeout(resolve, 1000));

    try {
        const response = await fetch('/User/Edit', {
            method: 'POST',
            body: formData
        });

        if (response.ok) {
            await showSuccessAlert("Guardado", "Los cambios se guardaron con exito.");

            window.location.href = '/Home';
        } else {
            throw new Error('Error al guardar los datos. Intentelo de nuevo.');
        }
    } catch (error) {
        await showErrorAlert("Error", error.message);
    } finally {
        Swal.close();
    }
}