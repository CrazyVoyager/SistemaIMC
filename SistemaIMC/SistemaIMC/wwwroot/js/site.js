document.addEventListener('DOMContentLoaded', function () {
// =======================================================
// 1. LÓGICA DEL MODO OSCURO
// =======================================================
const darkModeToggle = document.getElementById('darkModeToggle');
const darkModeIcon = document.getElementById('darkModeIcon');
const darkModeText = document.getElementById('darkModeText');
const htmlElement = document.documentElement;

// Función para aplicar el tema
function applyTheme(theme) {
    htmlElement.setAttribute('data-bs-theme', theme);
        
    if (theme === 'dark') {
        if (darkModeIcon) {
            darkModeIcon.classList.remove('fa-moon');
            darkModeIcon.classList.add('fa-sun');
        }
        if (darkModeText) {
            darkModeText.textContent = 'Modo Claro';
        }
    } else {
        if (darkModeIcon) {
            darkModeIcon.classList.remove('fa-sun');
            darkModeIcon.classList.add('fa-moon');
        }
        if (darkModeText) {
            darkModeText.textContent = 'Modo Oscuro';
        }
    }
}

// Cargar tema guardado o detectar preferencia del sistema
function loadSavedTheme() {
    const savedTheme = localStorage.getItem('imcinador-theme');
        
    if (savedTheme) {
        applyTheme(savedTheme);
    } else {
        // Detectar preferencia del sistema
        const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
        applyTheme(prefersDark ? 'dark' : 'light');
    }
}

// Toggle del modo oscuro
if (darkModeToggle) {
    darkModeToggle.addEventListener('click', function () {
        const currentTheme = htmlElement.getAttribute('data-bs-theme');
        const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
            
        applyTheme(newTheme);
        localStorage.setItem('imcinador-theme', newTheme);
            
        // Animación del botón
        this.style.transform = 'scale(0.95)';
        setTimeout(() => {
            this.style.transform = 'scale(1)';
        }, 150);
    });
}

// Cargar tema al iniciar
loadSavedTheme();

// Escuchar cambios en la preferencia del sistema
window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
    if (!localStorage.getItem('imcinador-theme')) {
        applyTheme(e.matches ? 'dark' : 'light');
    }
});

// =======================================================
// 2. LÓGICA DEL MENÚ (SIN CAMBIOS)
// =======================================================
const sidebar = document.getElementById('sidebar');
const toggleButton = document.getElementById('sidebarToggle');

    if (sidebar && toggleButton) {
        function toggleSidebar() {
            sidebar.classList.toggle('sidebar-hidden');
            if (sidebar.classList.contains('sidebar-hidden')) {
                toggleButton.innerHTML = '<i class="bi bi-list"></i> Mostrar Menú';
            } else {
                toggleButton.innerHTML = '<i class="bi bi-x-lg"></i> Ocultar Menú';
            }
        }

        toggleButton.addEventListener('click', toggleSidebar);

        if (window.innerWidth < 768) {
            sidebar.classList.add('sidebar-hidden');
            toggleButton.innerHTML = '<i class="bi bi-list"></i> Mostrar Menú';
        }

        window.addEventListener('resize', function () {
            if (window.innerWidth >= 768) {
                sidebar.classList.remove('sidebar-hidden');
                toggleButton.innerHTML = '<i class="bi bi-x-lg"></i> Ocultar Menú';
            }
        });
    }

    // =======================================================
    // 2. LÓGICA DE RUT (EVENTOS)
    // =======================================================

    // 🎯 CAMBIO AQUÍ: Ahora selecciona los inputs con name="RUT" Y el input con name="searchRut"
    const rutInputs = document.querySelectorAll('input[name="RUT"], input[name="searchRut"]');

    rutInputs.forEach(rutInput => {
        rutInput.addEventListener('input', function (e) {
            // Se actualiza el valor en tiempo real aplicando las reglas
            e.target.value = formatRutWithDashOnly(e.target.value);
        });

        rutInput.addEventListener('blur', function (e) {
            e.target.value = formatRutWithDashOnly(e.target.value);
        });
    });
});

// =======================================================
// 3. FUNCIONES GLOBALES DE RUT (LÓGICA CORREGIDA)
// =======================================================

/**
 * Formatea el RUT aplicando reglas estrictas:
 * - Solo números y K.
 * - La K solo puede ir al final.
 * - El guion se añade solo al completar 9 caracteres.
 */
function formatRutWithDashOnly(rut) {
    // 1. Limpieza básica: Solo permitir números y 'k' o 'K'
    let value = rut.replace(/[^0-9kK]/g, '').toUpperCase();

    // 2. REGLA CRÍTICA: Eliminar cualquier 'K' que NO esté al final.
    // La expresión regular /K(?!$)/g significa: "Busca una K que no sea seguida por el final de la cadena".
    // Esto convierte "KKKK" en "K" y "1K2" en "12".
    value = value.replace(/K(?!$)/g, '');

    // 3. REGLA EXTRA: Un RUT no puede ser solo "K". Debe tener números antes.
    // Si el usuario escribe "K" al principio, se borra.
    if (value === 'K') {
        value = '';
    }

    // 4. Truncar: Aseguramos que no pase de 9 caracteres (8 cuerpo + 1 DV)
    if (value.length > 9) {
        value = value.substring(0, 9);
    }

    // 5. Formatear: Solo si la longitud es EXACTAMENTE 9 (RUT completo)
    if (value.length === 9) {
        let cuerpo = value.substring(0, 8);
        let digitoVerificador = value.substring(8, 9);

        // Retorna: 12345678-K o 12345678-9
        return `${cuerpo}-${digitoVerificador}`;
    }

    // Si aún no está completo, devuelve el valor limpio sin guion
    return value;
}



/**
 * Exporta datos a Excel con indicador de carga
 * @param {string} actionName - Nombre de la acción en el controlador
 * @param {string} controllerName - Nombre del controlador
 * @param {Object} parameters - Parámetros adicionales
 */
function exportarAExcel(actionName, controllerName, parameters = {}) {
    try {
        // Mostrar indicador de carga
        const btn = event.target.closest('button');
        const textoOriginal = btn.innerHTML;
        btn.disabled = true;
        btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Exportando...';

        // Construir URL
        let url = `/${controllerName}/${actionName}? `;
        const queryParams = new URLSearchParams(parameters);
        url += queryParams.toString();

        // Descargar después de un pequeño delay
        setTimeout(() => {
            window.location.href = url;

            // Restaurar botón después de 2 segundos
            setTimeout(() => {
                btn.disabled = false;
                btn.innerHTML = textoOriginal;
            }, 2000);
        }, 500);

    } catch (error) {
        console.error('Error al exportar:', error);
        alert('Error al exportar los datos.  Por favor, intenta nuevamente.');
        const btn = event.target.closest('button');
        btn.disabled = false;
    }
}

/**
 * Exporta manteniendo los filtros actuales con indicador de carga
 * @param {string} actionName - Nombre de la acción
 * @param {string} controllerName - Nombre del controlador
 */
function exportarConFiltros(actionName, controllerName) {
    try {
        const btn = event.target.closest('button');
        const textoOriginal = btn.innerHTML;
        btn.disabled = true;
        btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Exportando...';

        const currentParams = new URLSearchParams(window.location.search);
        let url = `/${controllerName}/${actionName}?`;
        url += currentParams.toString();

        setTimeout(() => {
            window.location.href = url;

            setTimeout(() => {
                btn.disabled = false;
                btn.innerHTML = textoOriginal;
            }, 2000);
        }, 500);

    } catch (error) {
        console.error('Error al exportar con filtros:', error);
        alert('Error al exportar los datos. Por favor, intenta nuevamente.');
        const btn = event.target.closest('button');
        btn.disabled = false;
    }
}