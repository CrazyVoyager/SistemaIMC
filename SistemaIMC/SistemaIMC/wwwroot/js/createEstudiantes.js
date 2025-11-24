// Archivo: wwwroot/js/nutricion.js

document.addEventListener("DOMContentLoaded", function () {

    // Referencias a los elementos del DOM
    const selectEstudiante = document.getElementById("ID_Estudiante");
    const fechaNacimientoInput = document.getElementById("FechaNacimientoDisplay");

    // Ruta al controlador para obtener la fecha de nacimiento
    // Se asume que el controlador es T_MedicionNutricionalController.
    const URL_API = "/T_MedicionNutricional/GetFechaNacimiento";

    selectEstudiante.addEventListener("change", async function () {

        const idEstudiante = this.value;

        // 1. Limpiar el campo de fecha de nacimiento si no hay selección válida
        fechaNacimientoInput.value = '';
        if (!idEstudiante) return;

        try {
            // 2. Hacemos el Fetch llamando a la acción del controlador
            const response = await fetch(`${URL_API}?idEstudiante=${idEstudiante}`);

            // Verificar si la respuesta de red fue correcta
            if (!response.ok) {
                throw new Error(`Error en la red: ${response.status}`);
            }

            // 3. Convertimos la respuesta a JSON
            // Se espera un objeto con la propiedad 'fechaNacimiento' (e.g., { fechaNacimiento: "YYYY-MM-DD" })
            const data = await response.json();

            // 4. Mostramos la fecha en el campo de solo lectura
            if (data && data.fechaNacimiento) {
                // Asignar el valor devuelto (formato "YYYY-MM-DD") al input type="date"
                fechaNacimientoInput.value = data.fechaNacimiento;
            } else {
                fechaNacimientoInput.value = 'No disponible';
            }

        } catch (error) {
            console.error("Hubo un problema al cargar la fecha de nacimiento:", error);
            fechaNacimientoInput.value = 'Error de carga';
            // Opcional: Mostrar un mensaje de alerta al usuario
            // alert("No se pudo cargar la fecha de nacimiento. Intente nuevamente.");
        }
    });
});