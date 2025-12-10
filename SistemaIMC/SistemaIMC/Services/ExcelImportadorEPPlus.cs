using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using OfficeOpenXml;

namespace SistemaIMC.Services
{
    /// <summary>
    /// Helper para leer hojas Excel (.xlsx) usando EPPlus y convertir en estructuras
    /// que el ServicioImportacionMasiva pueda procesar.
    /// </summary>
    public static class ExcelImportadorEPPlus
    {
        // Convierte la primera hoja de un stream .xlsx en una lista de diccionarios (header -> valor)
        public static List<Dictionary<string, string?>> ReadSheetAsDictionary(Stream excelStream, string sheetName = null)
        {
            // EPPlus requiere configurar LicenseContext en Program.cs:
            // ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(excelStream);
            var worksheet = string.IsNullOrWhiteSpace(sheetName) ? package.Workbook.Worksheets.FirstOrDefault() : package.Workbook.Worksheets[sheetName];
            var rows = new List<Dictionary<string, string?>>();
            if (worksheet == null || worksheet.Dimension == null) return rows;

            int lastCol = worksheet.Dimension.End.Column;
            int lastRow = worksheet.Dimension.End.Row;

            // Leer encabezados (fila 1)
            var headers = new List<string>(lastCol);
            for (int c = 1; c <= lastCol; c++)
            {
                var h = worksheet.Cells[1, c].Text?.Trim();
                headers.Add(string.IsNullOrWhiteSpace(h) ? $"Column{c}" : h!);
            }

            // Leer filas desde la fila 2
            for (int r = 2; r <= lastRow; r++)
            {
                var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
                bool anyValue = false;
                for (int c = 1; c <= lastCol; c++)
                {
                    var cell = worksheet.Cells[r, c];
                    string? val = null;
                    if (cell != null && !string.IsNullOrWhiteSpace(cell.Text))
                    {
                        if (cell.Value is DateTime dt)
                        {
                            val = dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            val = cell.Text?.Trim();
                        }

                        if (!string.IsNullOrWhiteSpace(val)) anyValue = true;
                    }
                    dict[headers[c - 1]] = val;
                }

                if (anyValue) rows.Add(dict);
            }

            return rows;
        }

        public static List<RegistroCsvEstudiante> ToRegistroEstudiantes(List<Dictionary<string, string?>> rows)
        {
            var list = new List<RegistroCsvEstudiante>();
            foreach (var d in rows)
            {
                list.Add(new RegistroCsvEstudiante
                {
                    RUT = Get(d, "RUT"),
                    NombreCompleto = Get(d, "NombreCompleto"),
                    FechaNacimiento = Get(d, "FechaNacimiento"),
                    ID_Sexo = Get(d, "ID_Sexo"),
                    ID_Establecimiento = Get(d, "ID_Establecimiento") ?? Get(d, "Establecimiento"),
                    NombreEstablecimiento = Get(d, "NombreEstablecimiento"),
                    ID_Curso = Get(d, "ID_Curso"),
                    NombreCurso = Get(d, "NombreCurso") ?? Get(d, "Curso"),
                    EstadoRegistro = Get(d, "EstadoRegistro")
                });
            }
            return list;
        }

        public static List<RegistroCsvMedicion> ToRegistroMediciones(List<Dictionary<string, string?>> rows)
        {
            var list = new List<RegistroCsvMedicion>();
            foreach (var d in rows)
            {
                list.Add(new RegistroCsvMedicion
                {
                    RUT = Get(d, "RUT"),
                    ID_Estudiante = Get(d, "ID_Estudiante"),
                    FechaMedicion = Get(d, "FechaMedicion"),
                    Peso_kg = Get(d, "Peso_kg"),
                    Estatura_cm = Get(d, "Estatura_cm"),
                    ID_DocenteEncargado = Get(d, "ID_DocenteEncargado"),
                    DocenteRUT = Get(d, "DocenteRUT"),
                    Observaciones = Get(d, "Observaciones")
                });
            }
            return list;
        }

        private static string? Get(Dictionary<string, string?> d, string key)
        {
            if (d.TryGetValue(key, out var v)) return v;
            // alias posibles
            if (d.TryGetValue(key.ToLower(), out v)) return v;
            if (d.TryGetValue(key.ToUpper(), out v)) return v;
            return null;
        }
    }
}