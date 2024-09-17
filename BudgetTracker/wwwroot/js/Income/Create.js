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

async function CreateIncome() {
    const form = document.getElementById('createIncomeForm');
    const formData = new FormData(form);

    const loadingSwal = showLoadingAlert('Creando ingreso');

    await new Promise(resolve => setTimeout(resolve, 1000)); 

    const dataObject = {};
    formData.forEach((value, key) => {
        dataObject[key] = value;
    });

    try {
        IsValidIncome(dataObject['amount'], dataObject['date']);

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