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
    const form = document.getElementById('editBillForm');
    const formData = new FormData(form);

    const loadingSwal = showLoadingAlert('Guardando gasto');

    await new Promise(resolve => setTimeout(resolve, 1000));

    const dataObject = {};
    formData.forEach((value, key) => {
        dataObject[key] = value;
    });

    try {
        IsValidBill(dataObject['amount'], dataObject['date']);

        const response = await fetch('/Bill/Edit', {
            method: 'POST',
            body: formData
        });

        const data = await response.json();

        if (response.ok) {
            await showSuccessAlert("Guardado", data.message);

            window.location.href = `/Bill/Details/?selectedDate=${dataObject['date']}`;
        } else {
            throw new Error(data.message);
        }
    } catch (error) {
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