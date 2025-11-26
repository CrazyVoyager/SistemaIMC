// Archivo: wwwroot/js/editarEstudiante.js

document.addEventListener("DOMContentLoaded", function () {
    // 1. Referencias a los elementos del DOM (usando los IDs generados por asp-for)
    const selectEstablecimiento = document.getElementById("ddlEstablecimiento");
    const selectCurso = document.getElementById("ddlCurso");


    // Campo oculto: Debe contener el ID_Curso actual del estudiante (ver paso 2).
    const initialCursoIdInput = document.getElementById("ID_Curso_Inicial");

    // Verificar que los elementos existan
    if (!selectEstablecimiento || !selectCurso || !initialCursoIdInput) {
        console.warn("Elementos DOM (ddlEstablecimiento, ddlCurso o ID_Curso_Inicial) no encontrados. Saliendo de la lógica de edición.");
        return;
    }

    const URL_API_CURSOS = "/T_Estudiante/GetCursosByEstablecimiento";
    const initialCursoId = initialCursoIdInput.value;

    // Función auxiliar para limpiar el dropdown de Curso
    function resetCursoDropdown() {
        selectCurso.innerHTML = '<option value="">Seleccionar Curso</option>';
        selectCurso.disabled = true;
    }

    // Función principal para cargar y seleccionar cursos (puede ser usada para la carga inicial o el evento change)
    async function loadAndSelectCursos(idEstablecimiento, cursoASeleccionarId) {

        // Limpiar y mostrar "Cargando"
        selectCurso.innerHTML = '<option value="">Cargando Cursos...</option>';
        selectCurso.disabled = true;

        if (!idEstablecimiento) {
            resetCursoDropdown();
            return;
        }

        try {
            const response = await fetch(`${URL_API_CURSOS}?idEstablecimiento=${idEstablecimiento}`);

            if (!response.ok) {
                throw new Error(`Error al obtener los cursos. Código: ${response.status}`);
            }

            const cursos = await response.json();

            // Limpiar y añadir la opción por defecto
            selectCurso.innerHTML = '<option value="">Seleccionar Curso</option>';

            if (cursos && cursos.length > 0) {

                cursos.forEach(curso => {
                    const option = document.createElement('option');
                    option.value = curso.id;
                    option.textContent = curso.name;

                    // Si se pasó un ID de curso, lo seleccionamos
                    if (cursoASeleccionarId && curso.id.toString() === cursoASeleccionarId.toString()) {
                        option.selected = true;
                    }

                    selectCurso.appendChild(option);
                });
                selectCurso.disabled = false; // Habilitar el dropdown
            } else {
                selectCurso.innerHTML = '<option value="">No hay cursos disponibles</option>';
            }

        } catch (error) {
            console.error("Hubo un problema al cargar los cursos:", error);
            selectCurso.innerHTML = '<option value="">Error de carga</option>';
        }
    }


    // ⭐ 1. LÓGICA DE INICIALIZACIÓN AL CARGAR LA PÁGINA ⭐
    const initialEstablecimientoId = selectEstablecimiento.value;

    if (initialEstablecimientoId) {
        // Carga inicial: usamos el Establecimiento actual y el Curso a seleccionar (desde el campo oculto)
        loadAndSelectCursos(initialEstablecimientoId, initialCursoId);
    } else {
        // Si por alguna razón el Establecimiento no tiene valor, limpiamos el Curso.
        resetCursoDropdown();
    }


    // ⭐ 2. LÓGICA DE INTERACCIÓN DEL USUARIO (Al cambiar el Establecimiento) ⭐
    selectEstablecimiento.addEventListener("change", function () {
        // Al cambiar por el usuario, no hay curso "a seleccionar" previo.
        loadAndSelectCursos(this.value, null);
    });
});