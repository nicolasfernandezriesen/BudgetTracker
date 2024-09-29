function IsValidIncome(amount, categoryId, date) {
    if (amount === undefined || amount <= 0) {
        throw new Error('El monto no puede ser menor a 0.');
    }

    if (date === undefined) {
        throw new Error('La fecha no puede estar vacía.');
    }

    if (categoryId === undefined) {
        throw new Error('Se tiene que seleccionar una categoria.');
    }

    const currentDate = new Date();
    const twoMonthsAgo = new Date();
    twoMonthsAgo.setMonth(twoMonthsAgo.getMonth() - 2);

    const incomeDate = new Date(date);

    if (incomeDate <= twoMonthsAgo || incomeDate > currentDate) {
        throw new Error('La fecha no puede ser futura ni ser hace 2 o mas meses atras.');
    }
}

async function CreateIncome() {
    const form = document.getElementById('createIncomeForm');
    const formData = new FormData(form);

    const dataObject = {};
    formData.forEach((value, key) => {
        dataObject[key] = value;
    });

    try {
        IsValidIncome(dataObject['amount'], dataObject['categoryId'], dataObject['date']);

        const loadingSwal = showLoadingAlert('Creando ingreso');

        const response = await fetch('/Income/Create', {
            method: 'POST',
            body: formData
        });

        const data = await response.json();

        if (response.ok) {
            await showSuccessAlert('¡Éxito!', data.message);
            window.location.href = '/User';
        } else {
            throw new Error(data.message);
        }  
    } catch (error) {
        await showErrorAlert("Error", error.message);
    } finally {
        Swal.close();
    }
}