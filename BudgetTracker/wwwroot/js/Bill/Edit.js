function IsValidBill(amount, date) {
    if (amount === '' || amount < 0) {
        throw new Error('El monto no puede ser menor a 0.');
    }

    if (date === '') {
        throw new Error('La fecha no puede estar vacía.');
    }

    const currentDate = new Date();
    const twoMonthsAgo = new Date();
    twoMonthsAgo.setMonth(twoMonthsAgo.getMonth() - 2);

    const incomeDate = new Date(date);

    if (incomeDate <= twoMonthsAgo || incomeDate > currentDate) {
        throw new Error('La fecha no puede ser futura ni ser hace 2 o mas meses atras.');
    }
}

async function SaveEdit() {
    const isConfirmed = await showConfirmationAlert('Actualizar gasto', 'Estas seguro que queres actualizar este gasto?');

    if (!isConfirmed.isConfirmed) {
        return;
    }

    const form = document.getElementById('editBillForm');
    const formData = new FormData(form);

    const dataObject = {};
    formData.forEach((value, key) => {
        dataObject[key] = value;
    });

    try {
        IsValidBill(dataObject['BillsAmount'], dataObject['BillsDate']);

        const loadingSwal = showLoadingAlert('Guardando gasto');

        const response = await fetch('/Bill/Edit', {
            method: 'POST',
            body: formData
        });
        const data = await response.json();

        if (response.ok) {
            Swal.close();

            await showSuccessAlert("Guardado", data.message);
            window.location.href = `/Bill/Details/?selectedDate=${dataObject['BillsDate']}`;
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

function Cancel() {
    var billDateElement = document.getElementById('billDate');
    var billDate = billDateElement.getAttribute('data-bill-date');

    window.location.href = `/Bill/Details/?selectedDate=${billDate}`;
}