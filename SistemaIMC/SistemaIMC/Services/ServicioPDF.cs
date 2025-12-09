using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaIMC.Models;
using System.Collections.Generic; // Necesario para List<>

namespace SistemaIMC.Services
{
    public class PdfExportService
    {
        // Actualizamos la firma para recibir la lista de mediciones
        public static byte[] ExportarFichaEstudiante(T_Estudiante estudiante, List<T_MedicionNutricional> mediciones)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // --- 1. Encabezado (Igual que antes) ---
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Ficha de Estudiante").Bold().FontSize(20).FontColor(Colors.Blue.Medium);
                            col.Item().Text("Sistema de Gestión IMC").FontSize(10).FontColor(Colors.Grey.Medium);
                        });
                        row.ConstantItem(100).AlignRight().Text(DateTime.Now.ToString("dd/MM/yyyy"));
                    });

                    // --- 2. Contenido ---
                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        // A. Datos Personales y Académicos (Tu código existente)
                        col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(20).Column(innerCol =>
                        {
                            innerCol.Item().Text("Datos Personales").FontSize(14).SemiBold().FontColor(Colors.Blue.Darken2);
                            innerCol.Item().PaddingBottom(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                            innerCol.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(150);
                                    columns.RelativeColumn();
                                });

                                // Función local helper
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

                            innerCol.Item().PaddingVertical(15);

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

                        // B. NUEVA SECCIÓN: Historial de Mediciones
                        col.Item().PaddingTop(20); // Separación de la caja anterior

                        col.Item().Text("Historial de Mediciones").FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().PaddingBottom(5).LineHorizontal(2).LineColor(Colors.Blue.Medium);

                        if (mediciones != null && mediciones.Any())
                        {
                            col.Item().PaddingTop(10).Table(table =>
                            {
                                // Definir columnas de la tabla
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(80);  // Fecha
                                    columns.RelativeColumn();    // Peso
                                    columns.RelativeColumn();    // Estatura
                                    columns.RelativeColumn();    // IMC
                                    columns.RelativeColumn(2);   // Diagnóstico/Categoría
                                });

                                // Encabezados de tabla
                                table.Header(header =>
                                {
                                    header.Cell().Element(EstiloCeldaHeader).Text("Fecha");
                                    header.Cell().Element(EstiloCeldaHeader).Text("Peso (kg)");
                                    header.Cell().Element(EstiloCeldaHeader).Text("Estatura (cm)");
                                    header.Cell().Element(EstiloCeldaHeader).Text("IMC");
                                    header.Cell().Element(EstiloCeldaHeader).Text("Estado Nutricional");

                                    // Estilo local para header
                                    static IContainer EstiloCeldaHeader(IContainer container)
                                    {
                                        return container.Background(Colors.Grey.Lighten3).Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Medium);
                                    }
                                });

                                // Filas de datos
                                foreach (var item in mediciones)
                                {
                                    table.Cell().Element(EstiloCelda).Text(item.FechaMedicion.ToString("dd/MM/yyyy"));
                                    table.Cell().Element(EstiloCelda).Text(item.Peso_kg.ToString("F1"));
                                    table.Cell().Element(EstiloCelda).Text(item.Estatura_cm.ToString("F0"));

                                    // Usamos la propiedad calculada IMC del modelo
                                    var imcValor = item.IMC.HasValue ? item.IMC.Value.ToString("F2") : "-";
                                    table.Cell().Element(EstiloCelda).Text(imcValor);

                                    // Mostramos el nombre de la categoría (ej: Obesidad Severa)
                                    var categoria = item.CategoriaIMC?.NombreCategoria ?? "Sin clasificar";
                                    table.Cell().Element(EstiloCelda).Text(categoria);

                                    static IContainer EstiloCelda(IContainer container)
                                    {
                                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten4).Padding(5);
                                    }
                                }
                            });
                        }
                        else
                        {
                            col.Item().PaddingTop(10).Text("No se registran mediciones para este estudiante.").Italic().FontColor(Colors.Grey.Darken1);
                        }
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