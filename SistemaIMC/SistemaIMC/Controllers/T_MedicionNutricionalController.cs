using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
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
    // Permite entrar a Admin, Director y Profesor (Index y Details visibles para todos ellos)
    [Authorize(Roles = "Administrador del Sistema, Supervisor / Director de la Entidad, Profesor / Encargado de Mediciones")]
    public class T_MedicionNutricionalController : Controller
    {
        private readonly TdDbContext _context;
        private const int DOCENTE_ROL_ID = 2;
        private const string REFERENCIA_OMS = "Patrones OMS 5-19 años";

        // Constantes para validación de rangos realistas
        private const decimal PESO_MINIMO_KG = 10m;
        private const decimal PESO_MAXIMO_KG = 150m;
        private const decimal ESTATURA_MINIMA_CM = 80m;
        private const decimal ESTATURA_MAXIMA_CM = 220m;

        public T_MedicionNutricionalController(TdDbContext context)
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

        /// <summary>
        /// Valida que los valores de peso y estatura estén dentro de rangos realistas.
        /// </summary>
        private bool ValidarRangosMedicion(decimal peso, decimal estatura, out string mensajeError)
        {
            mensajeError = string.Empty;

            if (peso < PESO_MINIMO_KG || peso > PESO_MAXIMO_KG)
            {
                mensajeError = $"El peso debe estar entre {PESO_MINIMO_KG} y {PESO_MAXIMO_KG} kg. Verifique el valor ingresado.";
                return false;
            }

            if (estatura < ESTATURA_MINIMA_CM || estatura > ESTATURA_MAXIMA_CM)
            {
                mensajeError = $"La estatura debe estar entre {ESTATURA_MINIMA_CM} y {ESTATURA_MAXIMA_CM} cm. Verifique el valor ingresado.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Calcula la categoría IMC basada en el valor del IMC (clasificación de respaldo).
        /// Se usa cuando el SP no asigna una categoría o como verificación adicional.
        /// </summary>
        private async Task<int?> ObtenerCategoriaIMCPorValor(decimal imc)
        {
            // Clasificación simplificada basada en IMC para adultos/adolescentes
            // Nota: La clasificación real debería usar Z-Score según OMS para niños
            string nombreCategoria;

            if (imc < 16.0m)
                nombreCategoria = "Bajo peso severo";
            else if (imc < 17.0m)
                nombreCategoria = "Bajo peso moderado";
            else if (imc < 18.5m)
                nombreCategoria = "Bajo peso";
            else if (imc < 25.0m)
                nombreCategoria = "Normal";
            else if (imc < 30.0m)
                nombreCategoria = "Sobrepeso";
            else if (imc < 35.0m)
                nombreCategoria = "Obesidad";
            else
                nombreCategoria = "Obesidad severa";

            // Obtener la primera palabra para buscar coincidencias parciales
            string palabraClave = nombreCategoria.Split(' ')[0].ToLower();

            // Buscar la categoría en la base de datos - traemos todas y filtramos en memoria
            var categorias = await _context.T_CategoriaIMCs.ToListAsync();
            var categoria = categorias.FirstOrDefault(c => 
                c.NombreCategoria.ToLower().Contains(palabraClave));

            return categoria?.ID_CategoriaIMC;
        }

        /// <summary>
        /// Aplica clasificación de respaldo si el SP no asignó una categoría.
        /// </summary>
        private async Task AplicarClasificacionRespaldo(int idMedicion, decimal imc)
        {
            var medicion = await _context.T_MedicionNutricional.FindAsync(idMedicion);
            if (medicion != null && medicion.ID_CategoriaIMC == null)
            {
                var categoriaId = await ObtenerCategoriaIMCPorValor(imc);
                if (categoriaId.HasValue)
                {
                    medicion.ID_CategoriaIMC = categoriaId.Value;
                    await _context.SaveChangesAsync();
                }
            }
        }

        private void PopulateDropdowns(T_MedicionNutricional? medicion = null, int? establecimientoFiltro = null)
        {
            try
            {
                var estudiantesQuery = _context.T_Estudiante
                    .Where(e => e.EstadoRegistro);

                // Filtrar por establecimiento si se especifica
                if (establecimientoFiltro.HasValue)
                {
                    estudiantesQuery = estudiantesQuery.Where(e => e.ID_Establecimiento == establecimientoFiltro.Value);
                }

                var estudiantes = estudiantesQuery
                    .Select(e => new { e.ID_Estudiante, e.NombreCompleto })
                    .OrderBy(e => e.NombreCompleto);
                ViewData["ID_Estudiante"] = new SelectList(estudiantes, "ID_Estudiante", "NombreCompleto", medicion?.ID_Estudiante);
            }
            catch
            {
                ViewData["ID_Estudiante"] = new SelectList(new List<object>(), "ID_Estudiante", "NombreCompleto");
            }

            try
            {
                var docentesQuery = _context.T_Usuario
                    .Where(u => u.ID_Rol == DOCENTE_ROL_ID && u.EstadoRegistro);

                // Filtrar por establecimiento si se especifica
                if (establecimientoFiltro.HasValue)
                {
                    docentesQuery = docentesQuery.Where(u => u.ID_Establecimiento == establecimientoFiltro.Value);
                }

                var docentes = docentesQuery
                    .Select(d => new { d.ID_Usuario, d.Nombre })
                    .OrderBy(d => d.Nombre);
                ViewData["ID_DocenteEncargado"] = new SelectList(docentes, "ID_Usuario", "Nombre", medicion?.ID_DocenteEncargado);
            }
            catch
            {
                ViewData["ID_DocenteEncargado"] = new SelectList(new List<object>(), "ID_Usuario", "Nombre");
            }
        }

        // GET: T_MedicionNutricional/Create - SOLO Admin y Profesor
        [Authorize(Roles = "Administrador del Sistema, Profesor / Encargado de Mediciones")]
        public IActionResult Create(int? ID_Estudiante)
        {
            // 2. Crea una instancia temporal para transportar el ID
            var nuevaMedicion = new T_MedicionNutricional();

            if (ID_Estudiante.HasValue)
            {
                nuevaMedicion.ID_Estudiante = ID_Estudiante.Value;
            }

            // Obtener establecimiento del usuario para filtrar los dropdowns
            var establecimientoUsuario = GetEstablecimientoUsuarioLogueado();
            var esAdmin = EsAdministrador();

            // 3. Pasa el modelo a PopulateDropdowns con filtro de establecimiento si aplica
            PopulateDropdowns(nuevaMedicion, esAdmin ? null : establecimientoUsuario);

            // 4. Retorna la vista con el modelo. 
            // Esto asegura que el <select asp-for="ID_Estudiante"> tome el valor del modelo.
            return View(nuevaMedicion);
        }

        // POST: T_MedicionNutricional/Create - SOLO Admin y Profesor
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador del Sistema, Profesor / Encargado de Mediciones")]
        public async Task<IActionResult> Create([Bind("ID_Estudiante,FechaMedicion,ID_DocenteEncargado,Peso_kg,Estatura_cm,Observaciones")] T_MedicionNutricional t_MedicionNutricional)
        {
            var establecimientoUsuario = GetEstablecimientoUsuarioLogueado();
            var esAdmin = EsAdministrador();

            // Validar rangos de peso y estatura antes de continuar
            if (!ValidarRangosMedicion(t_MedicionNutricional.Peso_kg, t_MedicionNutricional.Estatura_cm, out string mensajeError))
            {
                ModelState.AddModelError(string.Empty, mensajeError);
                PopulateDropdowns(t_MedicionNutricional, esAdmin ? null : establecimientoUsuario);
                return View(t_MedicionNutricional);
            }

            if (ModelState.IsValid)
            {
                var estudiante = await _context.T_Estudiante.FindAsync(t_MedicionNutricional.ID_Estudiante);
                if (estudiante == null)
                {
                    ModelState.AddModelError("ID_Estudiante", "El estudiante seleccionado no es válido.");
                    PopulateDropdowns(t_MedicionNutricional, esAdmin ? null : establecimientoUsuario);
                    return View(t_MedicionNutricional);
                }

                // Verificar que el profesor solo pueda crear mediciones para estudiantes de su establecimiento
                if (!esAdmin && establecimientoUsuario.HasValue && estudiante.ID_Establecimiento != establecimientoUsuario.Value)
                {
                    return Forbid();
                }

                t_MedicionNutricional.ID_CategoriaIMC = null;
                t_MedicionNutricional.ZScore_IMC = 0.0m;
                t_MedicionNutricional.Edad_Meses_Medicion = 0;
                t_MedicionNutricional.Referencia_Normativa = REFERENCIA_OMS;

                t_MedicionNutricional.FechaRegistro = DateTime.Now;
                _context.Add(t_MedicionNutricional);
                await _context.SaveChangesAsync();

                // Calcular IMC para usarlo en clasificación de respaldo
                decimal imcCalculado = t_MedicionNutricional.IMC ?? 0;

                try
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "EXEC SP_ActualizarCategoriaIMC @ID_Medicion, @FechaMedicion, @FechaNacimiento, @ID_Sexo",
                        new SqlParameter("@ID_Medicion", t_MedicionNutricional.ID_Medicion),
                        new SqlParameter("@FechaMedicion", t_MedicionNutricional.FechaMedicion),
                        new SqlParameter("@FechaNacimiento", estudiante.FechaNacimiento),
                        new SqlParameter("@ID_Sexo", estudiante.ID_Sexo)
                    );
                }
                catch (Exception ex)
                {
                    // Log del error pero continuamos con clasificación de respaldo
                    System.Diagnostics.Debug.WriteLine($"Error en SP_ActualizarCategoriaIMC: {ex.Message}");
                }

                // Aplicar clasificación de respaldo si el SP no asignó categoría
                await AplicarClasificacionRespaldo(t_MedicionNutricional.ID_Medicion, imcCalculado);

                return RedirectToAction(nameof(Index));
            }

            PopulateDropdowns(t_MedicionNutricional, esAdmin ? null : establecimientoUsuario);
            return View(t_MedicionNutricional);
        }

        [HttpGet]
        public async Task<IActionResult> GetFechaNacimiento(int idEstudiante)
        {
            var estudiante = await _context.T_Estudiante
                .Where(e => e.ID_Estudiante == idEstudiante)
                .Select(e => new { e.FechaNacimiento })
                .FirstOrDefaultAsync();

            if (estudiante == null) return NotFound();
            return Json(new { fechaNacimiento = estudiante.FechaNacimiento.ToString("yyyy-MM-dd") });
        }

        // GET: T_MedicionNutricional/Edit/5 - SOLO Admin y Profesor
        [Authorize(Roles = "Administrador del Sistema, Profesor / Encargado de Mediciones")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var t_MedicionNutricional = await _context.T_MedicionNutricional
                .Include(m => m.Estudiante)
                .FirstOrDefaultAsync(m => m.ID_Medicion == id);
            if (t_MedicionNutricional == null) return NotFound();

            // Verificar que el usuario solo pueda editar mediciones de estudiantes de su establecimiento
            var establecimientoUsuario = GetEstablecimientoUsuarioLogueado();
            var esAdmin = EsAdministrador();
            
            if (!esAdmin && establecimientoUsuario.HasValue && t_MedicionNutricional.Estudiante?.ID_Establecimiento != establecimientoUsuario.Value)
            {
                return Forbid();
            }

            PopulateDropdowns(t_MedicionNutricional, esAdmin ? null : establecimientoUsuario);
            return View(t_MedicionNutricional);
        }

        // POST: T_MedicionNutricional/Edit/5 - SOLO Admin y Profesor
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador del Sistema, Profesor / Encargado de Mediciones")]
        public async Task<IActionResult> Edit(int id, [Bind("ID_Medicion,ID_Estudiante,FechaMedicion,ID_DocenteEncargado,Peso_kg,Estatura_cm,Observaciones,FechaRegistro")] T_MedicionNutricional t_MedicionNutricional)
        {
            if (id != t_MedicionNutricional.ID_Medicion) return NotFound();

            var establecimientoUsuario = GetEstablecimientoUsuarioLogueado();
            var esAdmin = EsAdministrador();

            // Validar rangos de peso y estatura antes de continuar
            if (!ValidarRangosMedicion(t_MedicionNutricional.Peso_kg, t_MedicionNutricional.Estatura_cm, out string mensajeError))
            {
                ModelState.AddModelError(string.Empty, mensajeError);
                PopulateDropdowns(t_MedicionNutricional, esAdmin ? null : establecimientoUsuario);
                return View(t_MedicionNutricional);
            }

            if (ModelState.IsValid)
            {
                var estudiante = await _context.T_Estudiante.FindAsync(t_MedicionNutricional.ID_Estudiante);
                if (estudiante == null)
                {
                    ModelState.AddModelError("ID_Estudiante", "El estudiante seleccionado no es válido.");
                    PopulateDropdowns(t_MedicionNutricional, esAdmin ? null : establecimientoUsuario);
                    return View(t_MedicionNutricional);
                }

                // Verificar que el profesor solo pueda editar mediciones de estudiantes de su establecimiento
                if (!esAdmin && establecimientoUsuario.HasValue && estudiante.ID_Establecimiento != establecimientoUsuario.Value)
                {
                    return Forbid();
                }

                t_MedicionNutricional.ID_CategoriaIMC = null;
                t_MedicionNutricional.ZScore_IMC = 0.0m;
                t_MedicionNutricional.Edad_Meses_Medicion = 0;
                t_MedicionNutricional.Referencia_Normativa = REFERENCIA_OMS;

                // Calcular IMC para usarlo en clasificación de respaldo
                decimal imcCalculado = t_MedicionNutricional.IMC ?? 0;

                try
                {
                    _context.Update(t_MedicionNutricional);
                    await _context.SaveChangesAsync();

                    await _context.Database.ExecuteSqlRawAsync(
                        "EXEC SP_ActualizarCategoriaIMC @ID_Medicion, @FechaMedicion, @FechaNacimiento, @ID_Sexo",
                        new SqlParameter("@ID_Medicion", t_MedicionNutricional.ID_Medicion),
                        new SqlParameter("@FechaMedicion", t_MedicionNutricional.FechaMedicion),
                        new SqlParameter("@FechaNacimiento", estudiante.FechaNacimiento),
                        new SqlParameter("@ID_Sexo", estudiante.ID_Sexo)
                    );

                    // Aplicar clasificación de respaldo si el SP no asignó categoría
                    await AplicarClasificacionRespaldo(t_MedicionNutricional.ID_Medicion, imcCalculado);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!T_MedicionNutricionalExists(t_MedicionNutricional.ID_Medicion)) return NotFound();
                    else throw;
                }
                catch (Exception ex)
                {
                    // Log del error del SP pero continuamos
                    System.Diagnostics.Debug.WriteLine($"Error en SP_ActualizarCategoriaIMC: {ex.Message}");
                    // Intentar clasificación de respaldo
                    await AplicarClasificacionRespaldo(t_MedicionNutricional.ID_Medicion, imcCalculado);
                }
                return RedirectToAction(nameof(Index));
            }

            PopulateDropdowns(t_MedicionNutricional, esAdmin ? null : establecimientoUsuario);
            return View(t_MedicionNutricional);
        }

        // GET: T_MedicionNutricional (Index) - Accesible para Admin, Director y Profesor
        public async Task<IActionResult> Index(int? RegionId, int? ComunaId, int? EstablecimientoId, int? CursoId, string searchRut)
        {
            // --- 0. Obtener el establecimiento del usuario logueado ---
            var establecimientoUsuario = GetEstablecimientoUsuarioLogueado();
            var esAdmin = EsAdministrador();

            // Si el usuario no es admin y tiene establecimiento asignado, forzar el filtro
            if (!esAdmin && establecimientoUsuario.HasValue)
            {
                EstablecimientoId = establecimientoUsuario.Value;
                ViewBag.MostrarFiltrosGeograficos = false;
                ViewBag.NombreEstablecimiento = (await _context.T_Establecimientos
                    .FirstOrDefaultAsync(e => e.ID_Establecimiento == establecimientoUsuario.Value))?.NombreEstablecimiento ?? "Mi Establecimiento";
            }
            else
            {
                ViewBag.MostrarFiltrosGeograficos = true;
            }

            // --- 1. Carga inicial de Dropdowns (solo la primera lista y precarga de seleccionados) ---
            var regiones = await _context.T_Region
                .OrderBy(r => r.NombreRegion)
                .ToListAsync();
            ViewBag.RegionId = new SelectList(regiones, "ID_Region", "NombreRegion", RegionId);

            // Inicializar los demás ViewBags para que la vista recupere la selección si aplica
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

            // Consulta base con inclusiones necesarias para el display y la navegación de filtros.
            ViewData["CurrentFilterRut"] = searchRut;

            var medicionesQuery = _context.T_MedicionNutricional
                // Incluir toda la cadena de navegación: Medicion -> Estudiante -> Curso -> Establecimiento -> Comuna -> Región
                .Include(m => m.Estudiante)
                    .ThenInclude(e => e!.Curso)
                        .ThenInclude(c => c!.Establecimiento)
                            .ThenInclude(e => e!.Comuna)
                                .ThenInclude(c => c!.Region)
                .Include(m => m.CategoriaIMC)
                .Include(m => m.DocenteEncargado)
                .Include(m => m.ClasificacionFinal)
                .AsQueryable();

            // Aplicar filtros de forma jerárquica (el filtro más específico anula los más generales)
            if (CursoId.HasValue && CursoId.Value > 0)
            {
                medicionesQuery = medicionesQuery.Where(m => m.Estudiante != null && m.Estudiante.ID_Curso == CursoId.Value);
            }
            else if (EstablecimientoId.HasValue && EstablecimientoId.Value > 0)
            {
                medicionesQuery = medicionesQuery.Where(m => m.Estudiante != null && m.Estudiante.Curso != null && m.Estudiante.Curso.ID_Establecimiento == EstablecimientoId.Value);
            }
            else if (ComunaId.HasValue && ComunaId.Value > 0)
            {
                medicionesQuery = medicionesQuery.Where(m => m.Estudiante != null && m.Estudiante.Curso != null && m.Estudiante.Curso.Establecimiento != null && m.Estudiante.Curso.Establecimiento.ID_Comuna == ComunaId.Value);
            }
            else if (RegionId.HasValue && RegionId.Value > 0)
            {
                medicionesQuery = medicionesQuery.Where(m => m.Estudiante != null && m.Estudiante.Curso != null && m.Estudiante.Curso.Establecimiento != null && m.Estudiante.Curso.Establecimiento.Comuna != null && m.Estudiante.Curso.Establecimiento.Comuna.ID_Region == RegionId.Value);
            }
            if (!string.IsNullOrEmpty(searchRut))
            {
                // Usamos Contains para que encuentre coincidencias parciales (ej: "123" encuentra "123456")
                // Opcional: .Trim() elimina espacios accidentales
                medicionesQuery = medicionesQuery.Where(m => m.Estudiante != null && m.Estudiante.RUT.Contains(searchRut.Trim()));
            }
            // Obtener la lista final
            var mediciones = await medicionesQuery.ToListAsync();
            return View(mediciones);
        }

        // GET: Details - Accesible para Admin, Director y Profesor
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var t_MedicionNutricional = await _context.T_MedicionNutricional
                .Include(m => m.Estudiante)          
                .Include(m => m.DocenteEncargado)    
                .Include(m => m.CategoriaIMC)         
                .Include(m => m.ClasificacionFinal)   
                .FirstOrDefaultAsync(m => m.ID_Medicion == id);

            if (t_MedicionNutricional == null) return NotFound();
            return View(t_MedicionNutricional);
        }

        // DELETE: SOLO Admin y Profesor
        [Authorize(Roles = "Administrador del Sistema, Profesor / Encargado de Mediciones")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var t_MedicionNutricional = await _context.T_MedicionNutricional.FirstOrDefaultAsync(m => m.ID_Medicion == id);
            if (t_MedicionNutricional == null) return NotFound();
            return View(t_MedicionNutricional);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador del Sistema, Profesor / Encargado de Mediciones")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var t_MedicionNutricional = await _context.T_MedicionNutricional.FindAsync(id);
            if (t_MedicionNutricional != null)
            {
                _context.T_MedicionNutricional.Remove(t_MedicionNutricional);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrador del Sistema, Supervisor / Director de la Entidad, Profesor / Encargado de Mediciones")]
        // Ubicación: T_MedicionNutricionalController.cs

        [HttpGet]
        public async Task<IActionResult> HistorialMedicionesModal(int? estudianteId)
        {
            if (estudianteId == null)
            {
                return BadRequest("ID de estudiante no proporcionado.");
            }

            // 1. Cargar la información del estudiante (incluyendo RUT)
            var estudiante = await _context.T_Estudiante
                .FirstOrDefaultAsync(e => e.ID_Estudiante == estudianteId);
                

            if (estudiante == null)
            {
                return NotFound();
            }

            // 2. Cargar las mediciones
            var mediciones = await _context.T_MedicionNutricional
                // Usamos .Include(m => m.ClasificacionFinal) para asegurar que tengamos los datos de clasificación
                .Include(m => m.ClasificacionFinal)
                .Include(m => m.CategoriaIMC).Where(m => m.ID_Estudiante == estudianteId)
                .OrderByDescending(m => m.FechaMedicion)
                .ToListAsync();

            // 3. Almacenar la información del estudiante en ViewData
            ViewData["NombreEstudiante"] = estudiante.NombreCompleto;
            ViewData["RUTEstudiante"] = estudiante.RUT;
            ViewData["ID_Estudiante"] = estudiante.ID_Estudiante;
            // 4. Devolver la vista parcial con los datos de las mediciones
            return PartialView("_HistorialMedicionesModal", mediciones);
        }


        [HttpGet]
        [Authorize(Roles = "Administrador del Sistema, Supervisor / Director de la Entidad, Profesor / Encargado de Mediciones")]
        public async Task<IActionResult> ExportarMediciones(int? RegionId, int? ComunaId, int? EstablecimientoId, int? CursoId, string searchRut)
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

                var medicionesQuery = _context.T_MedicionNutricional
                    .Include(m => m.Estudiante)
                        .ThenInclude(e => e!.Curso)
                            .ThenInclude(c => c!.Establecimiento)
                                .ThenInclude(e => e!.Comuna)
                                    .ThenInclude(c => c!.Region)
                    .Include(m => m.CategoriaIMC)
                    .Include(m => m.DocenteEncargado)
                    .Include(m => m.ClasificacionFinal)
                    .AsQueryable();

                // Aplicar filtros (misma lógica que Index)
                if (CursoId.HasValue && CursoId.Value > 0)
                {
                    medicionesQuery = medicionesQuery.Where(m => m.Estudiante != null && m.Estudiante.ID_Curso == CursoId.Value);
                }
                else if (EstablecimientoId.HasValue && EstablecimientoId.Value > 0)
                {
                    medicionesQuery = medicionesQuery.Where(m => m.Estudiante != null && m.Estudiante.Curso != null && m.Estudiante.Curso.ID_Establecimiento == EstablecimientoId.Value);
                }
                else if (ComunaId.HasValue && ComunaId.Value > 0)
                {
                    medicionesQuery = medicionesQuery.Where(m => m.Estudiante != null && m.Estudiante.Curso != null && m.Estudiante.Curso.Establecimiento != null && m.Estudiante.Curso.Establecimiento.ID_Comuna == ComunaId.Value);
                }
                else if (RegionId.HasValue && RegionId.Value > 0)
                {
                    medicionesQuery = medicionesQuery.Where(m => m.Estudiante != null && m.Estudiante.Curso != null && m.Estudiante.Curso.Establecimiento != null && m.Estudiante.Curso.Establecimiento.Comuna != null && m.Estudiante.Curso.Establecimiento.Comuna.ID_Region == RegionId.Value);
                }

                if (!string.IsNullOrEmpty(searchRut))
                {
                    medicionesQuery = medicionesQuery.Where(m => m.Estudiante != null && m.Estudiante.RUT.Contains(searchRut.Trim()));
                }

                var mediciones = await medicionesQuery.ToListAsync();

                // Crear DTO para exportación con información legible
                var medicionesExport = mediciones.Select(m => new
                {
                    RUTEstudiante = m.Estudiante?.RUT ?? "N/A",
                    NombreEstudiante = m.Estudiante?.NombreCompleto ?? "N/A",
                    FechaMedicion = m.FechaMedicion.ToString("dd/MM/yyyy"),
                    PesoKg = m.Peso_kg,
                    EstatuaCm = m.Estatura_cm,
                    IMC = Math.Round(m.IMC ?? 0, 2),
                    Categoria = m.CategoriaIMC?.NombreCategoria ?? "No clasificada",
                    ZScore = Math.Round(m.ZScore_IMC ?? 0, 2),
                    EdadMeses = m.Edad_Meses_Medicion,
                    Docente = m.DocenteEncargado?.Nombre ?? "N/A",
                    Observaciones = m.Observaciones ?? ""
                }).ToList();

                var excelBytes = ExcelExportService.ExportToExcel(medicionesExport, "Mediciones Nutricionales");
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Mediciones_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
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

        private bool T_MedicionNutricionalExists(int id)
        {
            return _context.T_MedicionNutricional.Any(e => e.ID_Medicion == id);
        }
    }
}