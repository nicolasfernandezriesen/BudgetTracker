 async function CreateIncome() {
    const form = document.getElementById('createIncomeForm');
    const formData = new FormData(form);

    const loadingSwal = showLoadingAlert('Creando ingreso');

    await new Promise(resolve => setTimeout(resolve, 1000)); 

     try {
         const response = await fetch('/Income/Create', {
             method: 'POST',
             body: formData
         });
         const data = await response.json(); // Asegúrate de esperar a que se convierta en JSON
         if (response.ok) {
             await showSuccessAlert('¡Éxito!', data.message); // Usa data.message aquí
             console.log("Ingreso creado correctamente.", data.message);
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