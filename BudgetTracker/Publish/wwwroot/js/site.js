function showSuccessAlert(title, text) {
    return Swal.fire({
        title: title || '¡Éxito!',
        text: text || 'Operación realizada correctamente.',
        icon: 'success',
        confirmButtonText: 'OK'
    });
};

function showLoadingAlert(title, text) {
    return Swal.fire({
        title: title || 'Cargando...',
        text: text || 'Por favor, espera un momento.',
        didOpen: () => {
            Swal.showLoading();
        },
        allowOutsideClick: false,
        allowEscapeKey: false,
        showConfirmButton: false,
        showCancelButton: false,
    });
};

function showErrorAlert(title, text) {
    return Swal.fire({
        title: title || 'Error',
        text: text || 'Hubo un problema con la operación.',
        icon: 'error',
        confirmButtonText: 'OK'
    });
};