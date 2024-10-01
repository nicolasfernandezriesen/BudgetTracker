function IsValidIncome(amount, date) {
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
    const form = document.getElementById('editIncomeForm');
    const formData = new FormData(form);

    const dataObject = {};
    formData.forEach((value, key) => {
        dataObject[key] = value;
    });


    try {
        IsValidIncome(dataObject['amount'], dataObject['date']);

        const loadingSwal = showLoadingAlert('Guardando ingreso');

        const response = await fetch('/Income/Edit', {
            method: 'POST',
            body: formData
        });

        if (response.ok) {
            await showSuccessAlert("Guardado", 'Se ha editado correctamente.');
            window.location.href = `/Income/Details/?selectedDate=${dataObject['date']}`;
        } else {
            throw new Error('Ah ocurrido un error, vuelve a intertarlo, si el error persiste, contacte a soporte.');
        }
    } catch (error) {
        await showErrorAlert("Error", error.message);
    } finally {
        Swal.close();
    }
}

function Cancel() {
    var incomeDateElement = document.getElementById('incomeDate');
    var incomeDate = incomeDateElement.getAttribute('data-income-date');

    window.location.href = `/Income/Details/?selectedDate=${incomeDate}`;
}