// Archivo: wwwroot/js/createEstudiantes.js

document.addEventListener("DOMContentLoaded", function () {
    // Selectores jerárquicos: Región > Comuna > Establecimiento > Curso
    const selectRegion = document.getElementById("ddlRegion");
    const selectComuna = document.getElementById("ddlComuna");
    const selectEstablecimiento = document.getElementById("ddlEstablecimiento");
    const selectCurso = document.getElementById("ddlCurso");

    // Si alguno no está, salir
    if (!selectRegion || !selectComuna || !selectEstablecimiento || !selectCurso) {
        console.warn("Alguno de los selects jerárquicos no fue encontrado. Revisa los ids.");
        return;
    }

    // Helpers para resetear combos siguientes
    function resetDropdown(dropdown, textoVacio = "Seleccione...") {
        dropdown.innerHTML = `<option value="">${textoVacio}</option>`;
        dropdown.disabled = true;
    }

    // --- REGION -> COMUNA ---
    selectRegion.addEventListener("change", async function () {
        const regionId = this.value;
        resetDropdown(selectComuna, "Seleccione Comuna");
        resetDropdown(selectEstablecimiento, "Seleccione Establecimiento");
        resetDropdown(selectCurso, "Seleccione Curso");

        if (!regionId) return;

        try {
            const resp = await fetch(`/T_Estudiante/GetComunasByRegion?regionId=${regionId}`);
            if (!resp.ok) throw new Error("No se pudo cargar comunas");
            const comunas = await resp.json();

            if (comunas.length > 0) {
                selectComuna.innerHTML = '<option value="">Seleccione Comuna</option>';
                comunas.forEach(comuna => {
                    const option = document.createElement('option');
                    option.value = comuna.id;
                    option.textContent = comuna.name;
                    selectComuna.appendChild(option);
                });
                selectComuna.disabled = false;
            } else {
                selectComuna.innerHTML = '<option value="">Sin comunas</option>';
            }
        } catch (err) {
            selectComuna.innerHTML = '<option value="">Error al cargar comunas</option>';
            console.error(err);
        }
    });

    // --- COMUNA -> ESTABLECIMIENTO ---
    selectComuna.addEventListener("change", async function () {
        const comunaId = this.value;
        resetDropdown(selectEstablecimiento, "Seleccione Establecimiento");
        resetDropdown(selectCurso, "Seleccione Curso");

        if (!comunaId) return;

        try {
            const resp = await fetch(`/T_Estudiante/GetEstablecimientosByComuna?comunaId=${comunaId}`);
            if (!resp.ok) throw new Error("No se pudo cargar establecimientos");
            const establecimientos = await resp.json();

            if (establecimientos.length > 0) {
                selectEstablecimiento.innerHTML = '<option value="">Seleccione Establecimiento</option>';
                establecimientos.forEach(est => {
                    const option = document.createElement('option');
                    option.value = est.id;
                    option.textContent = est.name;
                    selectEstablecimiento.appendChild(option);
                });
                selectEstablecimiento.disabled = false;
            } else {
                selectEstablecimiento.innerHTML = '<option value="">Sin establecimientos</option>';
            }
        } catch (err) {
            selectEstablecimiento.innerHTML = '<option value="">Error al cargar establecimientos</option>';
            console.error(err);
        }
    });

    // --- ESTABLECIMIENTO -> CURSO ---
    selectEstablecimiento.addEventListener("change", async function () {
        const idEstablecimiento = this.value;
        resetDropdown(selectCurso, "Seleccione Curso");

        if (!idEstablecimiento) return;

        try {
            const response = await fetch(`/T_Estudiante/GetCursosByEstablecimiento?establecimientoId=${idEstablecimiento}`);

            if (!response.ok) throw new Error("Error al obtener los cursos.");
            const cursos = await response.json();

            if (cursos.length > 0) {
                selectCurso.innerHTML = '<option value="">Seleccione Curso</option>';
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
            selectCurso.innerHTML = '<option value="">Error al cargar cursos</option>';
            console.error(error);
        }
    });

    // --- RESET AL INICIO ---
    resetDropdown(selectComuna, "Seleccione Comuna");
    resetDropdown(selectEstablecimiento, "Seleccione Establecimiento");
    resetDropdown(selectCurso, "Seleccione Curso");
});