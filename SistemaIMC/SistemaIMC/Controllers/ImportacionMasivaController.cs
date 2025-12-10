using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaIMC.Data;
using SistemaIMC.Services;
using System.IO;
using System.Threading.Tasks;

namespace SistemaIMC.Controllers
{
    [Authorize(Roles = "Administrador del Sistema, Profesor / Encargado de Mediciones")]
    public class ImportacionMasivaController : Controller
    {
        private readonly TdDbContext _context;
        private readonly ServicioImportacionMasiva _servicio;

        public ImportacionMasivaController(TdDbContext context, ServicioImportacionMasiva servicio)
        {
            _context = context;
            _servicio = servicio;
        }

        // GET: ImportacionMasiva/Estudiantes
        public IActionResult Estudiantes()
        {
            ViewBag.SampleXlsx = "Crea un .xlsx con encabezado: RUT,NombreCompleto,FechaNacimiento,ID_Sexo,ID_Establecimiento,NombreEstablecimiento,ID_Curso,NombreCurso,EstadoRegistro";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Estudiantes(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["BulkError"] = "No se ha enviado ningún archivo.";
                return RedirectToAction(nameof(Estudiantes));
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".xls")
            {
                TempData["BulkError"] = "Formato no soportado. Use .xlsx";
                return RedirectToAction(nameof(Estudiantes));
            }

            using var stream = file.OpenReadStream();
            var rows = ExcelImportadorEPPlus.ReadSheetAsDictionary(stream);
            var registros = ExcelImportadorEPPlus.ToRegistroEstudiantes(rows);

            var resultado = await _servicio.ImportarEstudiantesDesdeRegistrosAsync(registros);

            TempData["BulkResult"] = $"Creados: {resultado.Creados}, Actualizados: {resultado.Actualizados}";
            TempData["BulkErrors"] = string.Join("\n", resultado.Errores);
            return RedirectToAction(nameof(Estudiantes));
        }

        // GET: ImportacionMasiva/Mediciones
        public IActionResult Mediciones()
        {
            ViewBag.SampleXlsx = "Crea un .xlsx con encabezado: RUT,ID_Estudiante,FechaMedicion,Peso_kg,Estatura_cm,ID_DocenteEncargado,DocenteRUT,Observaciones";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Mediciones(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["BulkError"] = "No se ha enviado ningún archivo.";
                return RedirectToAction(nameof(Mediciones));
            }

            int? defaultDocenteId = null;
            var docenteClaim = User.FindFirst("ID_Usuario")?.Value;
            if (!string.IsNullOrWhiteSpace(docenteClaim) && int.TryParse(docenteClaim, out var docenteId))
            {
                defaultDocenteId = docenteId;
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".xls")
            {
                TempData["BulkError"] = "Formato no soportado. Use .xlsx";
                return RedirectToAction(nameof(Mediciones));
            }

            using var stream = file.OpenReadStream();
            var rows = ExcelImportadorEPPlus.ReadSheetAsDictionary(stream);
            var registros = ExcelImportadorEPPlus.ToRegistroMediciones(rows);

            var resultado = await _servicio.ImportarMedicionesDesdeRegistrosAsync(registros, defaultDocenteId);

            TempData["BulkResult"] = $"Mediciones creadas: {resultado.Creados}";
            TempData["BulkErrors"] = string.Join("\n", resultado.Errores);
            return RedirectToAction(nameof(Mediciones));
        }
    }
}