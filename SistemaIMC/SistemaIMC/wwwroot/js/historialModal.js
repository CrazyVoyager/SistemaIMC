// Ubicación: wwwroot/js/historialModal.js

$(document).ready(function () {
    // 1. Obtener la referencia al modal de Bootstrap
    var historialModal = document.getElementById('historialModal');

    // 2. Escuchar el evento que se dispara JUSTO antes de que el modal se muestre
    historialModal.addEventListener('show.bs.modal', function (event) {

        var button = event.relatedTarget;
        var estudianteId = button.getAttribute('data-estudiante-id');
        var modalContent = $("#historialModal .modal-content");

        if (estudianteId) {
            // URL para llamar a la acción que devuelve la vista parcial del historial
            var url = '/T_MedicionNutricional/HistorialMedicionesModal?estudianteId=' + estudianteId;

            // Mostrar el spinner de carga (estado inicial)
            modalContent.html(
                '<div class="modal-body text-center">' +
                '<div class="spinner-border text-primary" role="status"><span class="visually-hidden">Cargando...</span></div>' +
                '<p class="mt-2">Cargando historial de mediciones...</p>' +
                '</div>'
            );

            // 3. Usar fetch para cargar el contenido de forma asíncrona
            fetch(url)
                .then(response => {
                    if (!response.ok) {
                        throw new Error(`Error HTTP: ${response.status}`);
                    }
                    return response.text();
                })
                .then(htmlContent => {
                    // 4. Si es exitoso, inyectar el HTML en el modal
                    modalContent.html(htmlContent);

                    var nuevaUrl = '/T_MedicionNutricional/Create?ID_Estudiante=' + estudianteId;

                    // 5. Seleccionar el botón y actualizar el atributo href
                    $('#btnNuevaMedicionModal').attr('href', nuevaUrl);

                })
                .catch(error => {
                    // 6. Manejar errores (incluyendo 404 y 500)
                    console.error('Error al cargar el historial:', error);

                    var errorMessage = `Error al cargar el historial. (${error.message})`;

                    // Mostrar mensaje de error en el modal
                    modalContent.html(
                        '<div class="modal-header bg-danger text-white"><h5 class="modal-title">Error de Carga</h5><button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button></div>' +
                        '<div class="modal-body text-center">' +
                        `<div class="alert alert-danger" role="alert">${errorMessage}</div>` +
                        '</div>' +
                        '<div class="modal-footer"><button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cerrar</button></div>'
                    );
                });
        }
    });
});