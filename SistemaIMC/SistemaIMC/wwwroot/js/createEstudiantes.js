// Archivo: wwwroot/js/createEstudiantes.js

document.addEventListener("DOMContentLoaded", function () {

    const selectEstablecimiento = document.getElementById("ddlEstablecimiento");
    const selectCurso = document.getElementById("ddlCurso");

    // Verificar que los elementos existan
    if (!selectEstablecimiento || !selectCurso) {
        console.warn("Elementos ddlEstablecimiento o ddlCurso no encontrados. Saliendo de la lógica JS.");
        return;
    }

    const URL_API_CURSOS = "/T_Estudiante/GetCursosByEstablecimiento";

    function resetCursoDropdown() {
        selectCurso.innerHTML = '<option value="">Seleccionar Curso</option>';
        selectCurso.disabled = true;
    }

    //resetCursoDropdown();

    selectEstablecimiento.addEventListener("change", async function () {
        const idEstablecimiento = this.value;

        resetCursoDropdown();

        if (!idEstablecimiento) return;

        try {
            const response = await fetch(`${URL_API_CURSOS}?idEstablecimiento=${idEstablecimiento}`);

            if (!response.ok) {
                throw new Error(`Error al obtener los cursos. Código: ${response.status}`);
            }

            const cursos = await response.json();

            if (cursos && cursos.length > 0) {
                selectCurso.innerHTML = ''; // Limpiar totalmente
                cursos.forEach(curso => {
                    const option = document.createElement('option');
                    option.value = curso.id;

                    option.textContent = curso.name;

                    selectCurso.appendChild(option);
                });
                selectCurso.disabled = false;
            } else {
                selectCurso.innerHTML = '<option value="">No hay cursos disponibles</option>';
            }

        } catch (error) {
            console.error("Hubo un problema al cargar los cursos:", error);
            selectCurso.innerHTML = '<option value="">Error de carga</option>';
        }
    });
});