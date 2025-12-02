using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.IO;

namespace SistemaIMC.Services
{
    public class ExcelExportService
    {
        /// <summary>
        /// Exporta una colección de datos a Excel con formato profesional
        /// </summary>
        public static byte[] ExportToExcel<T>(List<T> data, string sheetName)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add(sheetName);

                // Cargar datos desde la lista
                if (data.Count > 0)
                {
                    worksheet.Cells.LoadFromCollection(data, PrintHeaders: true);

                    // Formato de encabezados
                    using (var headerRange = worksheet.Cells[1, 1, 1, worksheet.Dimension.Columns])
                    {
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Font.Color.SetColor(Color.White);
                        headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        headerRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 102, 204)); // Azul
                        headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        headerRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }

                    // Ajustar ancho de columnas automáticamente
                    worksheet.Cells.AutoFitColumns();

                    // Agregar bordes a todas las celdas con datos - FORMA CORRECTA PARA EPPLUS 7
                    var dataRange = worksheet.Cells[1, 1, worksheet.Dimension.Rows, worksheet.Dimension.Columns];

                    dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                return package.GetAsByteArray();
            }
        }
    }
}