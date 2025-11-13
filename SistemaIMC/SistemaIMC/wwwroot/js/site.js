document.addEventListener('DOMContentLoaded', function () {
    const sidebar = document.getElementById('sidebar');
    const toggleButton = document.getElementById('sidebarToggle');

    if (sidebar && toggleButton) {
        // Función para alternar la visibilidad del menú
        function toggleSidebar() {
            // Añade o quita la clase que tiene la animación CSS
            sidebar.classList.toggle('sidebar-hidden');

            // Cambiar el texto/ícono del botón
            if (sidebar.classList.contains('sidebar-hidden')) {
                toggleButton.innerHTML = '<i class="bi bi-list"></i> Mostrar Menú';
            } else {
                toggleButton.innerHTML = '<i class="bi bi-x-lg"></i> Ocultar Menú';
            }
        }

        // Asignar el evento click al botón
        toggleButton.addEventListener('click', toggleSidebar);

        // Lógica inicial: Ocultar menú al cargar si la pantalla es pequeña
        if (window.innerWidth < 768) {
            sidebar.classList.add('sidebar-hidden');
            toggleButton.innerHTML = '<i class="bi bi-list"></i> Mostrar Menú';
        }
        // Si es desktop, el menú estará abierto por defecto y el texto dirá 'Ocultar Menú' (por el HTML inicial)

        // Manejar el redimensionamiento
        window.addEventListener('resize', function () {
            if (window.innerWidth >= 768) {
                // Si pasamos a desktop, asegurar que el menú está visible por si estaba oculto
                sidebar.classList.remove('sidebar-hidden');
                toggleButton.innerHTML = '<i class="bi bi-x-lg"></i> Ocultar Menú';
            }
        });
    }
});