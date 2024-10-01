function IsValidBill(amount, categoryId, date) {
    if (amount === undefined || amount <= 0) {
        throw new Error('El monto no puede ser menor a 0.');
    }

    if (date === undefined) {
        throw new Error('La fecha no puede estar vacía.');
    }

    if (categoryId === undefined) {
        throw new Error('Se tiene que seleccionar una categoria.');
    }

    const twoMonthsAgo = new Date();
    const twoMonthsAfter = new Date();
    twoMonthsAgo.setMonth(twoMonthsAgo.getMonth() - 2);
    twoMonthsAfter.setMonth(twoMonthsAfter.getMonth() + 2);

    const incomeDate = new Date(date);

    if (incomeDate <= twoMonthsAgo || incomeDate > twoMonthsAfter) {
        throw new Error('La fecha no puede ser hace 2 o mas meses atras o 2 o mas meses adelante.');
    }
}

async function CreateBill() {
    const form = document.getElementById('createBillForm');
    const formData = new FormData(form);

    const dataObject = {};
    formData.forEach((value, key) => {
        dataObject[key] = value;
    });

    try {
        IsValidBill(dataObject['amount'], dataObject['categoryId'], dataObject['date']);

        const loadingSwal = showLoadingAlert('Creando gasto');

        const response = await fetch('/Bill/Create', {
            method: 'POST',
            body: formData,
        });

        if (response.ok) {
            await showSuccessAlert('¡Éxito!', 'El gasto se ha creado exitosamente.');
            window.location.href = '/User';
        } else {
            throw new Error('Ah ocurrido un error, vuelve a intertarlo, si el error persiste, contacte a soporte.');
        }
    } catch (error) {
        await showErrorAlert("Error", error.message);
    } finally {
        Swal.close();
    }
}