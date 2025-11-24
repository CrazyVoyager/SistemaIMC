document.addEventListener("DOMContentLoaded", function () {

    // Referencias a los elementos del DOM
    const selectEstudiante = document.getElementById("ID_Estudiante");
    const fechaNacimientoInput = document.getElementById("FechaNacimientoDisplay");

    // Ruta base al controlador. La acción es GetFechaNacimiento.
    const URL_API_BASE = "/T_MedicionNutricional/GetFechaNacimiento";

    selectEstudiante.addEventListener("change", async function () {

        const idEstudiante = this.value;

        // 1. Limpiar el campo de fecha de nacimiento y salir si no hay selección
        fechaNacimientoInput.value = '';
        if (!idEstudiante) return;

        try {
            // 2. Usar fetch directamente, construyendo la URL con el ID del estudiante
            // Ejemplo de URL construida: /T_MedicionNutricional/GetFechaNacimiento?idEstudiante=5
            const response = await fetch(`${URL_API_BASE}?idEstudiante=${idEstudiante}`);

            // 3. Verificar si la respuesta de red fue correcta (código 200-299)
            if (!response.ok) {
                throw new Error(`Error al obtener la fecha. Código: ${response.status}`);
            }

            // 4. Convertimos la respuesta a JSON
            const data = await response.json();

            // 5. Mostrar la fecha en el campo de solo lectura
            if (data && data.fechaNacimiento) {
                // El controlador devuelve la fecha en formato "YYYY-MM-DD"
                fechaNacimientoInput.value = data.fechaNacimiento;
            } else {
                fechaNacimientoInput.value = 'No disponible';
            }

        } catch (error) {
            console.error("Hubo un problema al cargar la fecha de nacimiento:", error);
            fechaNacimientoInput.value = 'Error de carga';
        }
    });
});