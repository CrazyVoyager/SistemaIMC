using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaIMC.Models;

namespace SistemaIMC.Services
{
    public class PdfExportService
    {
        // NOTA: Ahora recibimos UN SOLO estudiante, no una lista
        public static byte[] ExportarFichaEstudiante(T_Estudiante estudiante)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // --- 1. Encabezado ---
                    page.Header().Row(row =>
                    {
                        // Lado Izquierdo: Título
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Ficha de Estudiante").Bold().FontSize(20).FontColor(Colors.Blue.Medium);
                            col.Item().Text("Sistema de Gestión IMC").FontSize(10).FontColor(Colors.Grey.Medium);
                        });

                        // Lado Derecho: Fecha
                        row.ConstantItem(100).AlignRight().Text(DateTime.Now.ToString("dd/MM/yyyy"));
                    });

                    // --- 2. Contenido (Datos del alumno) ---
                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        // Caja con borde para los datos personales
                        col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(20).Column(innerCol =>
                        {
                            innerCol.Item().Text("Datos Personales").FontSize(14).SemiBold().FontColor(Colors.Blue.Darken2);
                            innerCol.Item().PaddingBottom(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                            // Usamos una tabla invisible para alinear "Etiqueta: Valor"
                            innerCol.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(150); // Ancho de la etiqueta
                                    columns.RelativeColumn();    // Ancho del valor
                                });

                                // Método local para filas
                                void FilaDato(string etiqueta, string valor)
                                {
                                    table.Cell().PaddingBottom(5).Text(etiqueta).Bold();
                                    table.Cell().PaddingBottom(5).Text(valor);
                                }

                                FilaDato("Nombre Completo:", estudiante.NombreCompleto);
                                FilaDato("RUT:", estudiante.RUT);
                                FilaDato("Fecha de Nacimiento:", estudiante.FechaNacimiento.ToString("dd/MM/yyyy"));
                                FilaDato("Sexo:", estudiante.Sexo?.Sexo ?? "No especificado");
                            });

                            innerCol.Item().PaddingVertical(15); // Espacio

                            innerCol.Item().Text("Información Académica").FontSize(14).SemiBold().FontColor(Colors.Blue.Darken2);
                            innerCol.Item().PaddingBottom(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                            innerCol.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(150);
                                    columns.RelativeColumn();
                                });

                                void FilaDato(string etiqueta, string valor)
                                {
                                    table.Cell().PaddingBottom(5).Text(etiqueta).Bold();
                                    table.Cell().PaddingBottom(5).Text(valor);
                                }

                                FilaDato("Establecimiento:", estudiante.Establecimiento?.NombreEstablecimiento ?? "N/A");
                                FilaDato("Comuna:", estudiante.Establecimiento?.Comuna?.NombreComuna ?? "N/A");
                                FilaDato("Curso Actual:", estudiante.Curso?.NombreCurso ?? "N/A");
                                FilaDato("Estado:", estudiante.EstadoRegistro ? "Activo" : "Inactivo");
                            });
                        });
                    });

                    // --- 3. Pie de página ---
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Documento generado automáticamente - Página ");
                        x.CurrentPageNumber();
                    });
                });
            })
            .GeneratePdf();
        }
    }
}