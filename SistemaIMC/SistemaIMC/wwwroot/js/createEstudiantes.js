document.addEventListener("DOMContentLoaded", function () {

    // Referencias a los elementos del DOM
    const selectEstablecimiento = document.getElementById("ddlEstablecimiento");
    const selectCurso = document.getElementById("ddlCurso");

    // Ruta al controlador (Hardcoded como pediste para que el JS se encargue)
    const URL_API = "/T_Estudiante/GetCursosByEstablecimiento";

    selectEstablecimiento.addEventListener("change", async function () {

        const idEstablecimiento = this.value;

        // 1. Limpiar y deshabilitar el combo de cursos preventivamente
        selectCurso.innerHTML = '<option value="">Seleccionar Curso</option>';
        selectCurso.disabled = true;

        // Si no hay selección válida, terminamos aquí
        if (!idEstablecimiento) return;

        try {
            // 2. Hacemos el Fetch llamando a la acción del controlador
            const response = await fetch(`${URL_API}?idEstablecimiento=${idEstablecimiento}`);

            // Verificar si la respuesta de red fue correcta
            if (!response.ok) {
                throw new Error(`Error en la red: ${response.status}`);
            }

            // 3. Convertimos la respuesta a JSON
            const cursos = await response.json();

            // 4. Llenamos el select con los datos recibidos
            cursos.forEach(curso => {
                const option = document.createElement("option");
                option.value = curso.id;  // Viene de tu Controller: new { id = c.ID_Curso ... }
                option.textContent = curso.name; // Viene de tu Controller: new { ... name = c.NombreCurso }
                selectCurso.appendChild(option);
            });

            // 5. Habilitamos el select
            selectCurso.disabled = false;

        } catch (error) {
            console.error("Hubo un problema al cargar los cursos:", error);
            alert("No se pudieron cargar los cursos. Intente nuevamente.");
        }
    });
});