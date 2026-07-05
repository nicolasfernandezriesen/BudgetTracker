document.addEventListener('DOMContentLoaded', function () {
    const editOptions = document.getElementById('editOptions');
    if (editOptions) {
        editOptions.addEventListener('change', function () {
            const selectedOption = this.value;
            const sections = document.querySelectorAll('.edit-section');
            const submitBtn = document.getElementById('submitBtn');

            sections.forEach(section => {
                section.style.display = 'none';
            });

            if (selectedOption) {
                document.getElementById(selectedOption + 'Section').style.display = 'block';
                submitBtn.style.display = 'block';
            } else if (submitBtn) {
                submitBtn.style.display = 'none';
            }
        });
    }

    if (window.__editPageInit && typeof syncThemeToggle === 'function') {
        syncThemeToggle(window.__editPageInit.isDarkTheme === true);
    }
});

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
    if (dataObject['Password'] > 0) {
        if (dataObject['Password'] && dataObject['Password'].length < 6) {
            errors.push('La contraseña debe tener al menos 6 caracteres.');
        }
        if (dataObject['Password'] && !digitRegex.test(dataObject['Password'])) {
            errors.push('La contraseña debe contener al menos un número.');
        }
        if (dataObject['Password'] && dataObject['Password'] !== dataObject['ConfirmPassword']) {
            errors.push('Las contraseñas no coinciden.');
        }
        if (dataObject['OldPassword'].trim() === '') {
            errors.push('La contraseña actual es obligatoria.');
        }
    }

    if (errors.length > 0) {
        throw new Error(errors.join('\n')); 
    }

    return;
};

async function sendEditUser() {
    const isConfirmed = await showConfirmationAlert('Editar usuario', 'Estas seguro que queres editar el usuario?');

    if (!isConfirmed.isConfirmed) {
        return;
    }

    const form = document.getElementById('editUserForm');
    const formData = new FormData(form);

    const dataObject = {};
    formData.forEach((value, key) => {
        dataObject[key] = value;
    });

    try {
        checkValidData(dataObject);

        const loadingSwal = showLoadingAlert();

        const response = await fetch('/User/Edit', {
            method: 'POST',
            body: formData
        });
        const data = await response.json();

        if (response.ok) {
            Swal.close();

            await showSuccessAlert("Guardado", data.message);

            window.location.href = '/User';
        } else {
            throw new Error(data.message);
        }
    } catch (error) {
        Swal.close();

        await showErrorAlert("Error", error.message);
    } finally {
        Swal.close();
    }
};

async function sendThemeUpdate() {
    const isConfirmed = await showConfirmationAlert('Guardar tema', 'Estas seguro que queres guardar el tema seleccionado?');

    if (!isConfirmed.isConfirmed) {
        return;
    }

    const themeSelect = document.getElementById('themeSelect');
    const isDark = themeSelect.value === 'true';
    const isTestRole = window.__editPageInit?.isTestRole === true;

    try {
        const loadingSwal = showLoadingAlert();

        const data = await saveTheme(isDark);

        Swal.close();

        await showSuccessAlert('Tema', data.message || data.Message);

        if (!isTestRole && data.savedToDatabase !== false && data.SavedToDatabase !== false) {
            window.location.href = '/User';
        }
    } catch (error) {
        Swal.close();
        await showErrorAlert('Error', error.message);
    }
}
