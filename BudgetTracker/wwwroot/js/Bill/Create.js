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
    const isConfirmed = await showConfirmationAlert('Creacion de gasto', 'Estas seguro que queres crear este gasto?');

    if (!isConfirmed.isConfirmed) {
        return;
    }

    const form = document.getElementById('createBillForm');
    const formData = new FormData(form);

    const dataObject = {};
    formData.forEach((value, key) => {
        dataObject[key] = value;
    });

    try {
        IsValidBill(dataObject['BillsAmount'], dataObject['CategoryId'], dataObject['BillsDate']);

        const loadingSwal = showLoadingAlert('Creando gasto');

        const response = await fetch('/Bill/Create', {
            method: 'POST',
            body: formData,
        });
        const data = await response.json();

        if (response.ok) {
            Swal.close();

            await showSuccessAlert('¡Éxito!', data.message);
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
}