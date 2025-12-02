using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaIMC.Data;
using SistemaIMC.Models;
using SistemaIMC.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaIMC.Controllers
{
    // Permite entrar a Admin, Director y Profesor (Index y Details visibles para todos)
    [Authorize(Roles = "Administrador del Sistema, Supervisor / Director de la Entidad, Profesor / Encargado de Mediciones")]
    public class T_EstudianteController : Controller
    {
        private readonly TdDbContext _context;

        public T_EstudianteController(TdDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? RegionId, int? ComunaId, int? EstablecimientoId, int? CursoId, string searchRut)
        {
            // --- 1. Carga inicial de Dropdowns y ViewBags ---

            // Cargar Regiones para el primer dropdown (siempre visible)
            var regiones = await _context.T_Region
                .OrderBy(r => r.NombreRegion)
                .ToListAsync();
            ViewBag.RegionId = new SelectList(regiones, "ID_Region", "NombreRegion", RegionId);

            // Inicializar los demás ViewBags para que la vista recupere la selección si aplica
            // Si RegionId tiene valor, la lógica JS de la vista se encargará de rellenar los demás
            ViewBag.Comunas = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text", ComunaId);
            ViewBag.Establecimientos = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text", EstablecimientoId);
            ViewBag.Cursos = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text", CursoId);

            // --- 2. Construcción de la Consulta y Aplicación de Filtros ---

            ViewData["CurrentFilterRut"] = searchRut;

            var estudiantesQuery = _context.T_Estudiante
                // Incluir toda la cadena de navegación para el display de la tabla y para el filtrado geográfico
                .Include(e => e.Establecimiento)
                    .ThenInclude(est => est.Comuna) // Incluye Comuna desde Establecimiento
                        .ThenInclude(com => com.Region) // Incluye Región desde Comuna
                .Include(e => e.Curso)
                .Include(e => e.Sexo)
                .AsQueryable();

            // Aplicar filtros de forma jerárquica (de más específico a más general)
            if (CursoId.HasValue && CursoId.Value > 0)
            {
                // Filtro por Curso (más específico)
                estudiantesQuery = estudiantesQuery.Where(e => e.ID_Curso == CursoId.Value);
            }
            else if (EstablecimientoId.HasValue && EstablecimientoId.Value > 0)
            {
                // Filtro por Establecimiento
                estudiantesQuery = estudiantesQuery.Where(e => e.ID_Establecimiento == EstablecimientoId.Value);
            }
            else if (ComunaId.HasValue && ComunaId.Value > 0)
            {
                // Filtro por Comuna (navegación requerida)
                estudiantesQuery = estudiantesQuery.Where(e => e.Establecimiento.ID_Comuna == ComunaId.Value);
            }
            else if (RegionId.HasValue && RegionId.Value > 0)
            {
                // Filtro por Región (navegación requerida)
                estudiantesQuery = estudiantesQuery.Where(e => e.Establecimiento.Comuna.ID_Region == RegionId.Value);
            }
            if (!string.IsNullOrEmpty(searchRut))
            {
                // El alias de la consulta es 'e' (de 'estudiantesQuery'). 
                // Se debe usar 'e.RUT' en lugar de 'Estudiante.RUT'.
                // Usamos Contains para que encuentre coincidencias parciales
                estudiantesQuery = estudiantesQuery.Where(e => e.RUT.Contains(searchRut.Trim()));
            }

            var estudiantesConRelaciones = await estudiantesQuery.ToListAsync();

            return View(estudiantesConRelaciones);
        }

        // GET: T_Estudiante/Details/
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_Estudiante = await _context.T_Estudiante
                .Include(e => e.Sexo)
                .Include(e => e.Establecimiento)
                .Include(e => e.Curso)
                .FirstOrDefaultAsync(m => m.ID_Estudiante == id);

            if (t_Estudiante == null)
            {
                return NotFound();
            }

            return View(t_Estudiante);
        }

        // CREATE (GET) - Solo Admin y Profesor
        [Authorize(Roles = "Administrador del Sistema, Profesor / Encargado de Mediciones")]
        public async Task<IActionResult> Create(int? ID_Estudiante) // ⭐️ AÑADIR ESTE PARÁMETRO
        {
            // 1. Lógica para preseleccionar el estudiante si el ID viene en la URL
            if (ID_Estudiante.HasValue)
            {
                // NOTA: Asumo que en el formulario Create.cshtml tienes un <select asp-for="ID_Estudiante" ...>

                // Obtener la lista completa de estudiantes (o solo los activos)
                var estudiantes = await _context.T_Estudiante
                    .Where(e => e.EstadoRegistro) // Ejemplo de filtro
                    .OrderBy(e => e.NombreCompleto)
                    .ToListAsync();

                // Cargar el SelectList, usando ID_Estudiante.Value para preseleccionar
                ViewData["ID_Estudiante"] = new SelectList(
                    estudiantes,
                    "ID_Estudiante",
                    "NombreCompleto",
                    ID_Estudiante.Value // <--- Esto fuerza la selección en la vista
                );
            }
            else
            {
                // Si no viene el ID, carga la lista sin preselección (código original o una versión simplificada)
                var estudiantes = await _context.T_Estudiante
                    .Where(e => e.EstadoRegistro)
                    .OrderBy(e => e.NombreCompleto)
                    .ToListAsync();

                ViewData["ID_Estudiante"] = new SelectList(
                    estudiantes,
                    "ID_Estudiante",
                    "NombreCompleto"
                );
            }

            // 2. Cargar los otros ViewBags
            ViewBag.ID_Establecimiento = new SelectList(await _context.T_Establecimientos.OrderBy(e => e.NombreEstablecimiento).ToListAsync(), "ID_Establecimiento", "NombreEstablecimiento");
            ViewBag.ID_Curso = new SelectList(await _context.T_Curso.OrderBy(c => c.NombreCurso).ToListAsync(), "ID_Curso", "NombreCurso");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador del Sistema, Profesor / Encargado de Mediciones")]
        public async Task<IActionResult> Create([Bind("ID_Estudiante,RUT,NombreCompleto,FechaNacimiento,ID_Sexo,ID_Establecimiento,ID_Curso,EstadoRegistro")] T_Estudiante t_Estudiante)
        {
            if (ModelState.IsValid)
            {
                _context.Add(t_Estudiante);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(t_Estudiante);
        }

        // EDIT - Solo Admin y Profesor
        [Authorize(Roles = "Administrador del Sistema, Profesor / Encargado de Mediciones")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var t_Estudiante = await _context.T_Estudiante.Include(e => e.Establecimiento).Include(e => e.Curso).Include(e => e.Sexo).FirstOrDefaultAsync(m => m.ID_Estudiante == id);
            if (t_Estudiante == null) return NotFound();

            ViewBag.ID_Establecimiento = new SelectList(await _context.T_Establecimientos.ToListAsync(), "ID_Establecimiento", "NombreEstablecimiento", t_Estudiante.ID_Establecimiento);
            var cursosActuales = await _context.T_Curso.Where(c => c.ID_Establecimiento == t_Estudiante.ID_Establecimiento).OrderBy(c => c.NombreCurso).ToListAsync();
            ViewBag.ID_Curso = new SelectList(cursosActuales, "ID_Curso", "NombreCurso", t_Estudiante.ID_Curso);
            ViewBag.ID_Sexo = new SelectList(await _context.T_Sexo.ToListAsync(), "ID_Sexo", "Sexo", t_Estudiante.ID_Sexo);
            return View(t_Estudiante);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador del Sistema, Profesor / Encargado de Mediciones")]
        public async Task<IActionResult> Edit(int id, [Bind("ID_Estudiante,RUT,NombreCompleto,FechaNacimiento,ID_Sexo,ID_Establecimiento,ID_Curso,EstadoRegistro")] T_Estudiante t_Estudiante)
        {
            if (id != t_Estudiante.ID_Estudiante) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(t_Estudiante);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!T_EstudianteExists(t_Estudiante.ID_Estudiante)) return NotFound(); else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(t_Estudiante);
        }

        // DELETE - Solo Admin y Profesor
        [Authorize(Roles = "Administrador del Sistema, Profesor / Encargado de Mediciones")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var t_Estudiante = await _context.T_Estudiante.FirstOrDefaultAsync(m => m.ID_Estudiante == id);
            if (t_Estudiante == null) return NotFound();
            return View(t_Estudiante);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador del Sistema, Profesor / Encargado de Mediciones")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var t_Estudiante = await _context.T_Estudiante.FindAsync(id);
            if (t_Estudiante != null) _context.T_Estudiante.Remove(t_Estudiante);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        [Authorize(Roles = "Administrador del Sistema, Supervisor / Director de la Entidad, Profesor / Encargado de Mediciones")]
        public async Task<IActionResult> ExportarEstudiantes(int? RegionId, int? ComunaId, int? EstablecimientoId, int? CursoId, string searchRut)
        {
            try
            {
                var estudiantesQuery = _context.T_Estudiante
                    .Include(e => e.Establecimiento)
                        .ThenInclude(est => est.Comuna)
                            .ThenInclude(com => com.Region)
                    .Include(e => e.Curso)
                    .Include(e => e.Sexo)
                    .AsQueryable();

                // Aplicar filtros (misma lógica que Index)
                if (CursoId.HasValue && CursoId.Value > 0)
                {
                    estudiantesQuery = estudiantesQuery.Where(e => e.ID_Curso == CursoId.Value);
                }
                else if (EstablecimientoId.HasValue && EstablecimientoId.Value > 0)
                {
                    estudiantesQuery = estudiantesQuery.Where(e => e.ID_Establecimiento == EstablecimientoId.Value);
                }
                else if (ComunaId.HasValue && ComunaId.Value > 0)
                {
                    estudiantesQuery = estudiantesQuery.Where(e => e.Establecimiento.ID_Comuna == ComunaId.Value);
                }
                else if (RegionId.HasValue && RegionId.Value > 0)
                {
                    estudiantesQuery = estudiantesQuery.Where(e => e.Establecimiento.Comuna.ID_Region == RegionId.Value);
                }

                if (!string.IsNullOrEmpty(searchRut))
                {
                    estudiantesQuery = estudiantesQuery.Where(e => e.RUT.Contains(searchRut.Trim()));
                }

                var estudiantes = await estudiantesQuery.ToListAsync();

                // Crear DTO para exportación con información legible
                var estudiantesExport = estudiantes.Select(e => new
                {
                    RUT = e.RUT,
                    NombreCompleto = e.NombreCompleto,
                    FechaNacimiento = e.FechaNacimiento.ToString("dd/MM/yyyy"),
                    Sexo = e.Sexo?.Sexo ?? "N/A",
                    Establecimiento = e.Establecimiento?.NombreEstablecimiento ?? "N/A",
                    Curso = e.Curso?.NombreCurso ?? "N/A",
                    Estado = e.EstadoRegistro ? "Activo" : "Inactivo"
                }).ToList();

                var excelBytes = ExcelExportService.ExportToExcel(estudiantesExport, "Estudiantes");
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Estudiantes_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al exportar: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetComunasByRegion(int regionId)
        {
            var comunas = await _context.T_Comunas
                .Where(c => c.ID_Region == regionId)
                .Select(c => new { id = c.ID_Comuna, name = c.NombreComuna })
                .OrderBy(c => c.name)
                .ToListAsync();

            return Json(comunas);
        }

        [HttpGet]
        public async Task<IActionResult> GetEstablecimientosByComuna(int comunaId)
        {
            var establecimientos = await _context.T_Establecimientos
                .Where(e => e.ID_Comuna == comunaId)
                .Select(e => new { id = e.ID_Establecimiento, name = e.NombreEstablecimiento })
                .OrderBy(e => e.name)
                .ToListAsync();

            return Json(establecimientos);
        }

        [HttpGet]
        public async Task<IActionResult> GetCursosByEstablecimiento(int establecimientoId)
        {
            var cursos = await _context.T_Curso
                .Where(c => c.ID_Establecimiento == establecimientoId)
                .Select(c => new { id = c.ID_Curso, name = c.NombreCurso })
                .OrderBy(c => c.name)
                .ToListAsync();

            return Json(cursos);
        }

        private bool T_EstudianteExists(int id) => _context.T_Estudiante.Any(e => e.ID_Estudiante == id);

    }
}