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
using System.Security.Claims;
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

        /// <summary>
        /// Obtiene el ID del establecimiento del usuario logueado desde los claims.
        /// Retorna null si el usuario es Administrador del Sistema (no tiene establecimiento asignado).
        /// </summary>
        private int? GetEstablecimientoUsuarioLogueado()
        {
            var establecimientoClaim = User.FindFirst("ID_Establecimiento")?.Value;
            if (!string.IsNullOrEmpty(establecimientoClaim) && int.TryParse(establecimientoClaim, out int establecimientoId))
            {
                return establecimientoId;
            }
            return null;
        }

        /// <summary>
        /// Verifica si el usuario actual es Administrador del Sistema.
        /// </summary>
        private bool EsAdministrador()
        {
            return User.IsInRole("Administrador del Sistema");
        }

        public async Task<IActionResult> Index(int? RegionId, int? ComunaId, int? EstablecimientoId, int? CursoId, string searchRut)
        {
            // --- 0. Obtener el establecimiento del usuario logueado ---
            var establecimientoUsuario = GetEstablecimientoUsuarioLogueado();
            var esAdmin = EsAdministrador();

            // Si el usuario no es admin y tiene establecimiento asignado, forzar el filtro
            if (!esAdmin && establecimientoUsuario.HasValue)
            {
                EstablecimientoId = establecimientoUsuario.Value;
                // No mostrar filtros de región/comuna/establecimiento para profesores
                ViewBag.MostrarFiltrosGeograficos = false;
                ViewBag.NombreEstablecimiento = (await _context.T_Establecimientos
                    .FirstOrDefaultAsync(e => e.ID_Establecimiento == establecimientoUsuario.Value))?.NombreEstablecimiento ?? "Mi Establecimiento";
            }
            else
            {
                ViewBag.MostrarFiltrosGeograficos = true;
            }

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
            
            // Cargar cursos del establecimiento del usuario si aplica
            if (!esAdmin && establecimientoUsuario.HasValue)
            {
                var cursosEstablecimiento = await _context.T_Curso
                    .Where(c => c.ID_Establecimiento == establecimientoUsuario.Value)
                    .OrderBy(c => c.NombreCurso)
                    .ToListAsync();
                ViewBag.Cursos = new SelectList(cursosEstablecimiento, "ID_Curso", "NombreCurso", CursoId);
            }
            else
            {
                ViewBag.Cursos = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text", CursoId);
            }

            // --- 2. Construcción de la Consulta y Aplicación de Filtros ---

            ViewData["CurrentFilterRut"] = searchRut;

            var estudiantesQuery = _context.T_Estudiante
                // Incluir toda la cadena de navegación para el display de la tabla y para el filtrado geográfico
                .Include(e => e.Establecimiento)
                    .ThenInclude(est => est!.Comuna) // Incluye Comuna desde Establecimiento
                        .ThenInclude(com => com!.Region) // Incluye Región desde Comuna
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
                estudiantesQuery = estudiantesQuery.Where(e => e.Establecimiento != null && e.Establecimiento.ID_Comuna == ComunaId.Value);
            }
            else if (RegionId.HasValue && RegionId.Value > 0)
            {
                // Filtro por Región (navegación requerida)
                estudiantesQuery = estudiantesQuery.Where(e => e.Establecimiento != null && e.Establecimiento.Comuna != null && e.Establecimiento.Comuna.ID_Region == RegionId.Value);
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

            // Verificar que el usuario solo pueda ver estudiantes de su establecimiento
            var establecimientoUsuario = GetEstablecimientoUsuarioLogueado();
            if (!EsAdministrador() && establecimientoUsuario.HasValue && t_Estudiante.ID_Establecimiento != establecimientoUsuario.Value)
            {
                return Forbid();
            }

            return View(t_Estudiante);
        }


// CREATE (GET)
[Authorize(Roles = "Administrador del Sistema, Profesor / Encargado de Mediciones")]
    public async Task<IActionResult> Create()
    {
        var establecimientoUsuario = GetEstablecimientoUsuarioLogueado();
        var esAdmin = EsAdministrador();

        if (!esAdmin && establecimientoUsuario.HasValue)
        {
            // Profesor: solo puede crear estudiantes en su establecimiento
            ViewBag.MostrarFiltrosGeograficos = false;
            var establecimiento = await _context.T_Establecimientos.FirstOrDefaultAsync(e => e.ID_Establecimiento == establecimientoUsuario.Value);
            ViewBag.NombreEstablecimiento = establecimiento?.NombreEstablecimiento ?? "Mi Establecimiento";
            ViewBag.ID_Establecimiento = new SelectList(
                new[] { establecimiento },
                "ID_Establecimiento",
                "NombreEstablecimiento",
                establecimientoUsuario.Value);
            
            // Cargar solo los cursos del establecimiento del profesor
            var cursosEstablecimiento = await _context.T_Curso
                .Where(c => c.ID_Establecimiento == establecimientoUsuario.Value)
                .OrderBy(c => c.NombreCurso)
                .ToListAsync();
            ViewBag.ID_Curso = new SelectList(cursosEstablecimiento, "ID_Curso", "NombreCurso");
        }
        else
        {
            // Administrador: puede ver todos los filtros
            ViewBag.MostrarFiltrosGeograficos = true;
            ViewBag.RegionId = new SelectList(await _context.T_Region.OrderBy(r => r.NombreRegion).ToListAsync(), "ID_Region", "NombreRegion");
            ViewBag.Comunas = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
            ViewBag.ID_Establecimiento = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
            ViewBag.ID_Curso = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
        }
        
        ViewBag.ID_Sexo = new SelectList(await _context.T_Sexo.ToListAsync(), "ID_Sexo", "Sexo");

        return View();
    }

    // CREATE (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador del Sistema, Profesor / Encargado de Mediciones")]
    public async Task<IActionResult> Create([Bind("RUT,NombreCompleto,FechaNacimiento,ID_Sexo,ID_Establecimiento,ID_Curso,EstadoRegistro")] T_Estudiante t_Estudiante)
    {
        var establecimientoUsuario = GetEstablecimientoUsuarioLogueado();
        var esAdmin = EsAdministrador();

        // Si es profesor, forzar el establecimiento al suyo
        if (!esAdmin && establecimientoUsuario.HasValue)
        {
            t_Estudiante.ID_Establecimiento = establecimientoUsuario.Value;
        }

        if (ModelState.IsValid)
        {
            // Verificar si ya existe un estudiante con el mismo RUT
            var rutExistente = await _context.T_Estudiante
                .AnyAsync(e => e.RUT == t_Estudiante.RUT);
    
            if (rutExistente)
            {
                ModelState.AddModelError("RUT", "Ya existe un estudiante registrado con este RUT.");
            }
            else
            {
                _context.Add(t_Estudiante);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        }
        
        // Si hay error, recarga selects necesarios para mostrar el formulario correctamente
        if (!esAdmin && establecimientoUsuario.HasValue)
        {
            ViewBag.MostrarFiltrosGeograficos = false;
            var establecimiento = await _context.T_Establecimientos.FirstOrDefaultAsync(e => e.ID_Establecimiento == establecimientoUsuario.Value);
            ViewBag.NombreEstablecimiento = establecimiento?.NombreEstablecimiento ?? "Mi Establecimiento";
            ViewBag.ID_Establecimiento = new SelectList(
                new[] { establecimiento },
                "ID_Establecimiento",
                "NombreEstablecimiento",
                establecimientoUsuario.Value);
            
            var cursosEstablecimiento = await _context.T_Curso
                .Where(c => c.ID_Establecimiento == establecimientoUsuario.Value)
                .OrderBy(c => c.NombreCurso)
                .ToListAsync();
            ViewBag.ID_Curso = new SelectList(cursosEstablecimiento, "ID_Curso", "NombreCurso", t_Estudiante.ID_Curso);
        }
        else
        {
            ViewBag.MostrarFiltrosGeograficos = true;
            ViewBag.RegionId = new SelectList(await _context.T_Region.OrderBy(r => r.NombreRegion).ToListAsync(), "ID_Region", "NombreRegion");
            ViewBag.Comunas = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
            ViewBag.ID_Establecimiento = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
            ViewBag.ID_Curso = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
        }
        
        ViewBag.ID_Sexo = new SelectList(await _context.T_Sexo.ToListAsync(), "ID_Sexo", "Sexo", t_Estudiante.ID_Sexo);

        return View(t_Estudiante);
    }

    // EDIT - Solo Admin y Profesor
    [Authorize(Roles = "Administrador del Sistema, Profesor / Encargado de Mediciones")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var t_Estudiante = await _context.T_Estudiante.Include(e => e.Establecimiento).Include(e => e.Curso).Include(e => e.Sexo).FirstOrDefaultAsync(m => m.ID_Estudiante == id);
            if (t_Estudiante == null) return NotFound();

            // Verificar que el usuario solo pueda editar estudiantes de su establecimiento
            var establecimientoUsuario = GetEstablecimientoUsuarioLogueado();
            var esAdmin = EsAdministrador();
            
            if (!esAdmin && establecimientoUsuario.HasValue && t_Estudiante.ID_Establecimiento != establecimientoUsuario.Value)
            {
                return Forbid();
            }

            if (!esAdmin && establecimientoUsuario.HasValue)
            {
                ViewBag.MostrarFiltrosGeograficos = false;
                ViewBag.NombreEstablecimiento = t_Estudiante.Establecimiento?.NombreEstablecimiento ?? "Mi Establecimiento";
            }
            else
            {
                ViewBag.MostrarFiltrosGeograficos = true;
            }

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

            // Verificar que el usuario solo pueda editar estudiantes de su establecimiento
            var establecimientoUsuario = GetEstablecimientoUsuarioLogueado();
            var esAdmin = EsAdministrador();
            
            if (!esAdmin && establecimientoUsuario.HasValue && t_Estudiante.ID_Establecimiento != establecimientoUsuario.Value)
            {
                return Forbid();
            }

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
            var t_Estudiante = await _context.T_Estudiante
                .Include(e => e.Establecimiento)
                .FirstOrDefaultAsync(m => m.ID_Estudiante == id);
            if (t_Estudiante == null) return NotFound();

            // Verificar que el usuario solo pueda eliminar estudiantes de su establecimiento
            var establecimientoUsuario = GetEstablecimientoUsuarioLogueado();
            if (!EsAdministrador() && establecimientoUsuario.HasValue && t_Estudiante.ID_Establecimiento != establecimientoUsuario.Value)
            {
                return Forbid();
            }

            return View(t_Estudiante);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador del Sistema, Profesor / Encargado de Mediciones")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var t_Estudiante = await _context.T_Estudiante.FindAsync(id);
            
            // Verificar que el usuario solo pueda eliminar estudiantes de su establecimiento
            var establecimientoUsuario = GetEstablecimientoUsuarioLogueado();
            if (!EsAdministrador() && establecimientoUsuario.HasValue && t_Estudiante?.ID_Establecimiento != establecimientoUsuario.Value)
            {
                return Forbid();
            }

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
                // Aplicar filtro por establecimiento del usuario si no es admin
                var establecimientoUsuario = GetEstablecimientoUsuarioLogueado();
                var esAdmin = EsAdministrador();

                if (!esAdmin && establecimientoUsuario.HasValue)
                {
                    EstablecimientoId = establecimientoUsuario.Value;
                }

                var estudiantesQuery = _context.T_Estudiante
                    .Include(e => e.Establecimiento)
                        .ThenInclude(est => est!.Comuna)
                            .ThenInclude(com => com!.Region)
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
                    estudiantesQuery = estudiantesQuery.Where(e => e.Establecimiento != null && e.Establecimiento.ID_Comuna == ComunaId.Value);
                }
                else if (RegionId.HasValue && RegionId.Value > 0)
                {
                    estudiantesQuery = estudiantesQuery.Where(e => e.Establecimiento != null && e.Establecimiento.Comuna != null && e.Establecimiento.Comuna.ID_Region == RegionId.Value);
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