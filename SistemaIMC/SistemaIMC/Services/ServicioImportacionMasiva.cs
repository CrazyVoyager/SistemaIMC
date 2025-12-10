using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SistemaIMC.Data;
using SistemaIMC.Models;

namespace SistemaIMC.Services
{
    // Estructuras de entrada (las usamos sin referencia a "CSV" para mantener nombres compatibles)
    public class RegistroCsvEstudiante
    {
        public string? RUT { get; set; }
        public string? NombreCompleto { get; set; }
        public string? FechaNacimiento { get; set; }
        public string? ID_Sexo { get; set; }
        public string? ID_Establecimiento { get; set; }
        public string? NombreEstablecimiento { get; set; }
        public string? ID_Curso { get; set; }
        public string? NombreCurso { get; set; }
        public string? EstadoRegistro { get; set; }
    }

    public class RegistroCsvMedicion
    {
        public string? RUT { get; set; }
        public string? ID_Estudiante { get; set; }
        public string? FechaMedicion { get; set; }
        public string? Peso_kg { get; set; }
        public string? Estatura_cm { get; set; }
        public string? ID_DocenteEncargado { get; set; }
        public string? DocenteRUT { get; set; }
        public string? Observaciones { get; set; }
    }

    public class ResultadoImportacionMasiva
    {
        public int Creados { get; set; } = 0;
        public int Actualizados { get; set; } = 0;
        public List<string> Errores { get; set; } = new List<string>();
    }

    public class ServicioImportacionMasiva
    {
        private readonly TdDbContext _context;

        public ServicioImportacionMasiva(TdDbContext context)
        {
            _context = context;
        }

        // Validar RUT chileno (DV)
        private bool ValidarRut(string rut)
        {
            if (string.IsNullOrWhiteSpace(rut)) return false;

            var clean = rut.Replace(".", "").Replace(" ", "").ToUpper();
            string numero;
            string dvInput;

            if (clean.Contains("-"))
            {
                var parts = clean.Split('-');
                if (parts.Length != 2) return false;
                numero = parts[0];
                dvInput = parts[1];
            }
            else
            {
                if (clean.Length < 2) return false;
                numero = clean.Substring(0, clean.Length - 1);
                dvInput = clean.Substring(clean.Length - 1);
            }

            if (!int.TryParse(numero, out _)) return false;

            int suma = 0;
            int factor = 2;
            for (int i = numero.Length - 1; i >= 0; i--)
            {
                suma += (numero[i] - '0') * factor;
                factor++;
                if (factor > 7) factor = 2;
            }
            int resto = suma % 11;
            int dvCalc = 11 - resto;
            string dvCalcStr;
            if (dvCalc == 11) dvCalcStr = "0";
            else if (dvCalc == 10) dvCalcStr = "K";
            else dvCalcStr = dvCalc.ToString();

            return dvCalcStr.Equals(dvInput, StringComparison.OrdinalIgnoreCase);
        }

        // ----------------------------
        // Importación de Estudiantes (desde registros ya parseados)
        // ----------------------------
        public async Task<ResultadoImportacionMasiva> ImportarEstudiantesDesdeRegistrosAsync(IEnumerable<RegistroCsvEstudiante> registrosEnumerable, CancellationToken cancellationToken = default)
        {
            var resultado = new ResultadoImportacionMasiva();
            var registros = registrosEnumerable.ToList();

            foreach (var (registro, indice) in registros.Select((r, i) => (r, i + 2)))
            {
                try
                {
                    // Validaciones básicas
                    if (string.IsNullOrWhiteSpace(registro.RUT))
                    {
                        resultado.Errores.Add($"Línea {indice}: RUT vacío.");
                        continue;
                    }
                    if (!ValidarRut(registro.RUT))
                    {
                        resultado.Errores.Add($"Línea {indice}: RUT inválido ('{registro.RUT}').");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(registro.NombreCompleto))
                    {
                        resultado.Errores.Add($"Línea {indice}: NombreCompleto vacío.");
                        continue;
                    }

                    if (!DateTime.TryParseExact(registro.FechaNacimiento, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaNac))
                    {
                        resultado.Errores.Add($"Línea {indice}: FechaNacimiento inválida ('{registro.FechaNacimiento}'). Use yyyy-MM-dd.");
                        continue;
                    }

                    if (!int.TryParse(registro.ID_Sexo, out var idSexo))
                    {
                        resultado.Errores.Add($"Línea {indice}: ID_Sexo inválido ('{registro.ID_Sexo}').");
                        continue;
                    }

                    // Establecimiento: preferir ID, si no usar NombreEstablecimiento
                    int idEstablecimiento;
                    if (!string.IsNullOrWhiteSpace(registro.ID_Establecimiento) && int.TryParse(registro.ID_Establecimiento, out var idEstTmp))
                    {
                        var est = await _context.T_Establecimientos.FindAsync(new object[] { idEstTmp }, cancellationToken);
                        if (est == null)
                        {
                            resultado.Errores.Add($"Línea {indice}: Establecimiento ID {idEstTmp} no encontrado.");
                            continue;
                        }
                        idEstablecimiento = idEstTmp;
                    }
                    else if (!string.IsNullOrWhiteSpace(registro.NombreEstablecimiento))
                    {
                        var estByName = await _context.T_Establecimientos.FirstOrDefaultAsync(e =>
                            e.NombreEstablecimiento.ToLower().Trim() == registro.NombreEstablecimiento.ToLower().Trim(), cancellationToken);
                        if (estByName == null)
                        {
                            resultado.Errores.Add($"Línea {indice}: Establecimiento con nombre '{registro.NombreEstablecimiento}' no encontrado.");
                            continue;
                        }
                        idEstablecimiento = estByName.ID_Establecimiento;
                    }
                    else
                    {
                        resultado.Errores.Add($"Línea {indice}: Falta ID_Establecimiento o NombreEstablecimiento.");
                        continue;
                    }

                    // Curso: preferir ID, si no usar NombreCurso (y validar pertenencia)
                    int idCurso;
                    if (!string.IsNullOrWhiteSpace(registro.ID_Curso) && int.TryParse(registro.ID_Curso, out var idCursoTmp))
                    {
                        var curso = await _context.T_Curso.FindAsync(new object[] { idCursoTmp }, cancellationToken);
                        if (curso == null)
                        {
                            resultado.Errores.Add($"Línea {indice}: Curso ID {idCursoTmp} no encontrado.");
                            continue;
                        }

                        // comparar pertenencia (curso.ID_Establecimiento puede ser nullable o no)
                        if (curso.ID_Establecimiento != idEstablecimiento)
                        {
                            resultado.Errores.Add($"Línea {indice}: Curso ID {idCursoTmp} no pertenece al establecimiento ID {idEstablecimiento}.");
                            continue;
                        }
                        idCurso = idCursoTmp;
                    }
                    else if (!string.IsNullOrWhiteSpace(registro.NombreCurso))
                    {
                        var cursoByName = await _context.T_Curso.FirstOrDefaultAsync(c =>
                            c.NombreCurso.ToLower().Trim() == registro.NombreCurso.ToLower().Trim()
                            && (c.ID_Establecimiento == idEstablecimiento), cancellationToken);
                        if (cursoByName == null)
                        {
                            resultado.Errores.Add($"Línea {indice}: Curso con nombre '{registro.NombreCurso}' no encontrado en el establecimiento ID {idEstablecimiento}.");
                            continue;
                        }
                        idCurso = cursoByName.ID_Curso;
                    }
                    else
                    {
                        resultado.Errores.Add($"Línea {indice}: Falta ID_Curso o NombreCurso.");
                        continue;
                    }

                    // EstadoRegistro
                    bool estadoRegistro = registro.EstadoRegistro switch
                    {
                        "1" or "true" or "True" => true,
                        "0" or "false" or "False" => false,
                        null or "" => true,
                        _ => registro.EstadoRegistro.Trim().ToLower() == "true" || registro.EstadoRegistro.Trim() == "1"
                    };

                    // Insertar o actualizar
                    var estudianteExistente = await _context.T_Estudiante.FirstOrDefaultAsync(e => e.RUT == registro.RUT, cancellationToken);
                    if (estudianteExistente != null)
                    {
                        estudianteExistente.NombreCompleto = registro.NombreCompleto;
                        estudianteExistente.FechaNacimiento = fechaNac;
                        estudianteExistente.ID_Sexo = idSexo;
                        estudianteExistente.ID_Establecimiento = idEstablecimiento;
                        estudianteExistente.ID_Curso = idCurso;
                        estudianteExistente.EstadoRegistro = estadoRegistro;

                        _context.T_Estudiante.Update(estudianteExistente);
                        resultado.Actualizados++;
                    }
                    else
                    {
                        var nuevo = new T_Estudiante
                        {
                            RUT = registro.RUT,
                            NombreCompleto = registro.NombreCompleto,
                            FechaNacimiento = fechaNac,
                            ID_Sexo = idSexo,
                            ID_Establecimiento = idEstablecimiento,
                            ID_Curso = idCurso,
                            EstadoRegistro = estadoRegistro
                        };
                        await _context.T_Estudiante.AddAsync(nuevo, cancellationToken);
                        resultado.Creados++;
                    }

                    // Persistir por fila para compatibilidad transaccional parcial
                    await _context.SaveChangesAsync(cancellationToken);
                }
                catch (Exception exFila)
                {
                    resultado.Errores.Add($"Línea {indice}: excepción procesando fila: {exFila.Message}");
                }
            }

            return resultado;
        }

        // ----------------------------
        // Importación de Mediciones (desde registros ya parseados)
        // ----------------------------
        public async Task<ResultadoImportacionMasiva> ImportarMedicionesDesdeRegistrosAsync(IEnumerable<RegistroCsvMedicion> registrosEnumerable, int? defaultDocenteId = null, CancellationToken cancellationToken = default)
        {
            var resultado = new ResultadoImportacionMasiva();
            var registros = registrosEnumerable.ToList();

            foreach (var (registro, indice) in registros.Select((r, i) => (r, i + 2)))
            {
                try
                {
                    // Localizar estudiante (RUT preferente)
                    T_Estudiante? estudiante = null;
                    if (!string.IsNullOrWhiteSpace(registro.RUT))
                    {
                        if (!ValidarRut(registro.RUT))
                        {
                            resultado.Errores.Add($"Línea {indice}: RUT estudiante inválido ('{registro.RUT}').");
                            continue;
                        }
                        estudiante = await _context.T_Estudiante.FirstOrDefaultAsync(e => e.RUT == registro.RUT, cancellationToken);
                        if (estudiante == null)
                        {
                            resultado.Errores.Add($"Línea {indice}: No se encontró estudiante con RUT '{registro.RUT}'.");
                            continue;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(registro.ID_Estudiante) && int.TryParse(registro.ID_Estudiante, out var idEst))
                    {
                        estudiante = await _context.T_Estudiante.FindAsync(new object[] { idEst }, cancellationToken);
                        if (estudiante == null)
                        {
                            resultado.Errores.Add($"Línea {indice}: No se encontró estudiante con ID {idEst}.");
                            continue;
                        }
                    }
                    else
                    {
                        resultado.Errores.Add($"Línea {indice}: Falta RUT o ID_Estudiante para localizar al estudiante.");
                        continue;
                    }

                    // Fecha de medición
                    if (!DateTime.TryParseExact(registro.FechaMedicion, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaMed))
                    {
                        resultado.Errores.Add($"Línea {indice}: FechaMedicion inválida ('{registro.FechaMedicion}'). Use yyyy-MM-dd.");
                        continue;
                    }
                    if (fechaMed > DateTime.Today)
                    {
                        resultado.Errores.Add($"Línea {indice}: FechaMedicion no puede ser futura ('{registro.FechaMedicion}').");
                        continue;
                    }

                    // Peso y estatura
                    if (!decimal.TryParse(registro.Peso_kg, NumberStyles.Any, CultureInfo.InvariantCulture, out var peso))
                    {
                        resultado.Errores.Add($"Línea {indice}: Peso_kg inválido ('{registro.Peso_kg}').");
                        continue;
                    }
                    if (!decimal.TryParse(registro.Estatura_cm, NumberStyles.Any, CultureInfo.InvariantCulture, out var estatura))
                    {
                        resultado.Errores.Add($"Línea {indice}: Estatura_cm inválida ('{registro.Estatura_cm}').");
                        continue;
                    }

                    // Rangos plausibles
                    const decimal PESO_MIN = 10.0m, PESO_MAX = 150.0m;
                    const decimal EST_MIN = 50.0m, EST_MAX = 220.0m;
                    if (peso < PESO_MIN || peso > PESO_MAX)
                    {
                        resultado.Errores.Add($"Línea {indice}: Peso fuera de rango ({peso}).");
                        continue;
                    }
                    if (estatura < EST_MIN || estatura > EST_MAX)
                    {
                        resultado.Errores.Add($"Línea {indice}: Estatura fuera de rango ({estatura}).");
                        continue;
                    }

                    // ID_DocenteEncargado o DocenteRUT
                    int idDocente = 0;
                    if (!string.IsNullOrWhiteSpace(registro.ID_DocenteEncargado) && int.TryParse(registro.ID_DocenteEncargado, out var idDocTmp))
                    {
                        var docente = await _context.T_Usuario.FindAsync(new object[] { idDocTmp }, cancellationToken);
                        if (docente == null)
                        {
                            resultado.Errores.Add($"Línea {indice}: Docente ID {idDocTmp} no encontrado.");
                            continue;
                        }
                        idDocente = idDocTmp;
                    }
                    else if (!string.IsNullOrWhiteSpace(registro.DocenteRUT))
                    {
                        if (!ValidarRut(registro.DocenteRUT))
                        {
                            resultado.Errores.Add($"Línea {indice}: DocenteRUT inválido ('{registro.DocenteRUT}').");
                            continue;
                        }
                        var docente = await _context.T_Usuario.FirstOrDefaultAsync(u => u.RUT == registro.DocenteRUT, cancellationToken);
                        if (docente == null)
                        {
                            resultado.Errores.Add($"Línea {indice}: Docente con RUT '{registro.DocenteRUT}' no encontrado.");
                            continue;
                        }
                        idDocente = docente.ID_Usuario;
                    }
                    else if (defaultDocenteId.HasValue)
                    {
                        idDocente = defaultDocenteId.Value;
                    }
                    else
                    {
                        resultado.Errores.Add($"Línea {indice}: Falta ID_DocenteEncargado o DocenteRUT, y no hay docente por defecto.");
                        continue;
                    }

                    // Crear medición y calcular IMC_Persistido
                    var medicion = new T_MedicionNutricional
                    {
                        ID_Estudiante = estudiante.ID_Estudiante,
                        FechaMedicion = fechaMed,
                        ID_DocenteEncargado = idDocente,
                        Peso_kg = Math.Round(peso, 2),
                        Estatura_cm = Math.Round(estatura, 2),
                        Observaciones = registro.Observaciones,
                        FechaRegistro = DateTime.Now
                    };

                    // Calcular IMC (cm -> m) y guardar en IMC_Persistido (mapeado a la columna IMC en BD)
                    if (medicion.Estatura_cm > 0)
                    {
                        var est_m = medicion.Estatura_cm / 100m;
                        var imcCalc = medicion.Peso_kg / (est_m * est_m);
                        medicion.IMC_Persistido = Math.Round(imcCalc, 4);
                    }
                    else
                    {
                        medicion.IMC_Persistido = 0m;
                    }

                    // Insertar y persistir para obtener ID_Medicion
                    await _context.T_MedicionNutricional.AddAsync(medicion, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    // Llamar al SP para actualizar categoría/zscore
                    try
                    {
                        var parametros = new[]
                        {
                            new SqlParameter("@ID_Medicion", medicion.ID_Medicion),
                            new SqlParameter("@FechaMedicion", medicion.FechaMedicion),
                            new SqlParameter("@FechaNacimiento", estudiante.FechaNacimiento),
                            new SqlParameter("@ID_Sexo", estudiante.ID_Sexo)
                        };

                        await _context.Database.ExecuteSqlRawAsync(
                            "EXEC dbo.SP_ActualizarCategoriaIMC @ID_Medicion, @FechaMedicion, @FechaNacimiento, @ID_Sexo",
                            parametros);
                    }
                    catch (Exception exSP)
                    {
                        resultado.Errores.Add($"Línea {indice}: Error ejecutando SP para medición ID {medicion.ID_Medicion}: {exSP.Message}");
                    }

                    resultado.Creados++;
                }
                catch (Exception exFila)
                {
                    resultado.Errores.Add($"Línea {indice}: excepción procesando fila: {exFila.Message}");
                }
            }

            return resultado;
        }
    }
}