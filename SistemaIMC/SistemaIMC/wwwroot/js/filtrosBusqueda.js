// Archivo: wwwroot/js/indexMedicionesFilters.js

document.addEventListener("DOMContentLoaded", function () {
    const ddlRegion = document.getElementById("ddlRegion");
    const ddlComuna = document.getElementById("ddlComuna");
    const ddlEstablecimiento = document.getElementById("ddlEstablecimiento");
    const ddlCurso = document.getElementById("ddlCurso");

    // URLs de las acciones API
    const URL_API_COMUNAS = "/T_MedicionNutricional/GetComunasByRegion";
    const URL_API_ESTABLECIMIENTOS = "/T_MedicionNutricional/GetEstablecimientosByComuna";
    const URL_API_CURSOS = "/T_MedicionNutricional/GetCursosByEstablecimiento";

    /**
     * Función para resetear y deshabilitar un dropdown
     */
    function resetDropdown(dropdown, defaultText, disabled = true) {
        dropdown.innerHTML = `<option value="">${defaultText}</option>`;
        dropdown.disabled = disabled;
    }

    /**
     * Función para llenar un dropdown con datos JSON.
     */
    function populateDropdown(dropdown, data, defaultText, selectedValue) {
        // La vista (cshtml) ya tiene cargados los valores iniciales.
        // Solo necesitamos poblar si se hace un cambio por JS.

        // Mantener la opción de filtro "Todos" o "Seleccionar"
        resetDropdown(dropdown, defaultText, false);

        if (data && data.length > 0) {
            data.forEach(item => {
                const option = document.createElement('option');
                option.value = item.id;
                option.textContent = item.name;
                // Si el valor actual coincide con el valor seleccionado, lo marca como seleccionado
                if (selectedValue && item.id.toString() === selectedValue.toString()) {
                    option.selected = true;
                }
                dropdown.appendChild(option);
            });
        } else {
            resetDropdown(dropdown, `No hay ${defaultText}s disponibles`, true);
        }
    }

    /**
     * Carga de datos AJAX
     */
    async function loadData(url, paramName, paramValue, targetDropdown, defaultText, selectedValue = null) {
        resetDropdown(targetDropdown, "Cargando...");

        try {
            const response = await fetch(`${url}?${paramName}=${paramValue}`);
            if (!response.ok) throw new Error("Error de red.");

            const data = await response.json();
            populateDropdown(targetDropdown, data, defaultText, selectedValue);

        } catch (error) {
            console.error("Error al cargar datos:", error);
            resetDropdown(targetDropdown, `Error de carga: ${defaultText}`, true);
        }
    }


    // ⭐ EVENTOS DE CAMBIO CASCADA ⭐

    ddlRegion.addEventListener("change", function () {
        const regionId = this.value;
        // Resetear y deshabilitar los niveles inferiores
        resetDropdown(ddlComuna, "Todas las Comunas");
        resetDropdown(ddlEstablecimiento, "Todos los Establecimientos");
        resetDropdown(ddlCurso, "Todos los Cursos");

        if (regionId) {
            loadData(URL_API_COMUNAS, 'regionId', regionId, ddlComuna, "Todas las Comunas");
        }
    });

    ddlComuna.addEventListener("change", function () {
        const comunaId = this.value;
        // Resetear y deshabilitar los niveles inferiores
        resetDropdown(ddlEstablecimiento, "Todos los Establecimientos");
        resetDropdown(ddlCurso, "Todos los Cursos");

        if (comunaId) {
            loadData(URL_API_ESTABLECIMIENTOS, 'comunaId', comunaId, ddlEstablecimiento, "Todos los Establecimientos");
        }
    });

    ddlEstablecimiento.addEventListener("change", function () {
        const establecimientoId = this.value;
        // Resetear y deshabilitar el nivel inferior
        resetDropdown(ddlCurso, "Todos los Cursos");

        if (establecimientoId) {
            loadData(URL_API_CURSOS, 'establecimientoId', establecimientoId, ddlCurso, "Todos los Cursos");
        }
    });

    // ⭐ LÓGICA DE INICIALIZACIÓN (Habilita el siguiente nivel si ya tiene un valor cargado) ⭐
    // Esto asegura que los dropdowns precargados por el servidor con los filtros activos sigan habilitados.

    if (ddlComuna.value) ddlComuna.disabled = false;
    if (ddlEstablecimiento.value) ddlEstablecimiento.disabled = false;
    if (ddlCurso.value) ddlCurso.disabled = false;
});