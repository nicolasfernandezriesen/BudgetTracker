﻿function previousOrNextCalendar(month, year) {
    if (month == 13) {
        month = 1;
        year++;
    }
    else if (month == 0) {
        month = 12;
        year--;
     }
    window.location.href = "/History/GetBillIncomeAndMonthlyTotal?month=" + month + "&year=" + year;
}

function goBackPage(date = null) {

    if (date == null) {
        window.history.back();
    } else {
        var partes = date.split("/");

        var month = partes[1];
        var year = partes[2];
        window.location.href = "/History/GetBillIncomeAndMonthlyTotal?month=" + month + "&year=" + year;
    }
}
function Delete(type, id) {
    Swal.fire({
        title: "¿Estás seguro?",
        text: "¡Una vez borrado no hay vuelta atrás!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Sí, ¡bórralo!"
    }).then((result) => {
        if (result.isConfirmed) {
            if (type === 'bill') {
                DeleteBill(id);
            } else if (type === 'income') {
                DeleteIncome(id);
            }
        }
    });
}

async function DeleteBill(id) {
    const form = document.getElementById(`deleteFromr_${id}`);
    const formData = new FormData(form);

    const dataObject = {};
    formData.forEach((value, key) => {
        dataObject[key] = value;
    });

    try {
        const loadingSwal = showLoadingAlert('Borrando gasto');

        const response = await fetch('/Bill/Delete', {
            method: 'POST',
            body: formData
        });

        if (response.ok) {
            await showSuccessAlert("Borrado", 'El gasto se borro exitosamente.');
            goBackPage(dataObject['date']);
        } else {
            throw new Error('Ah ocurrido un error, vuelve a intertarlo, si el error persiste, contacte a soporte.');
        }
    } catch (error) {
        await showErrorAlert("Error", error.message);
    } finally {
        Swal.close(loadingSwal);
    }
}

async function DeleteIncome(id) {
    const form = document.getElementById(`deleteFromr_${id}`);
    const formData = new FormData(form);

    const dataObject = {};
    formData.forEach((value, key) => {
        dataObject[key] = value;
    });

    try {
        const loadingSwal = showLoadingAlert('Borrando ingreso');

        const response = await fetch('/Income/Delete', {
            method: 'POST',
            body: formData
        });
        const data = await response.json();

        if (response.ok) {
            await showSuccessAlert("Borrado", 'El ingreso se borro exitosamente.');
            goBackPage(dataObject['date']);
        } else {
            throw new Error('Ah ocurrido un error, vuelve a intertarlo, si el error persiste, contacte a soporte.');
        }
    } catch (error) {
        await showErrorAlert("Error", error.message);
    } finally {
        Swal.close(loadingSwal);
    }
}