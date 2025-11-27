using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SistemaIMC.Data;
using SistemaIMC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public T_MedicionNutricionalController(TdDbContext context)
        {
            _context = context;
        }

        private void PopulateDropdowns(T_MedicionNutricional? medicion = null)
        {
            try
            {
                var estudiantes = _context.T_Estudiante
                    .Where(e => e.EstadoRegistro)
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
                var docentes = _context.T_Usuario
                    .Where(u => u.ID_Rol == DOCENTE_ROL_ID && u.EstadoRegistro)
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
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }

        // POST: T_MedicionNutricional/Create - SOLO Admin y Profesor
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador del Sistema, Profesor / Encargado de Mediciones")]
        public async Task<IActionResult> Create([Bind("ID_Estudiante,FechaMedicion,ID_DocenteEncargado,Peso_kg,Estatura_cm,Observaciones")] T_MedicionNutricional t_MedicionNutricional)
        {
            if (ModelState.IsValid)
            {
                var estudiante = await _context.T_Estudiante.FindAsync(t_MedicionNutricional.ID_Estudiante);
                if (estudiante == null)
                {
                    ModelState.AddModelError("ID_Estudiante", "El estudiante seleccionado no es válido.");
                    PopulateDropdowns(t_MedicionNutricional);
                    return View(t_MedicionNutricional);
                }

                t_MedicionNutricional.ID_CategoriaIMC = null;
                t_MedicionNutricional.ZScore_IMC = 0.0m;
                t_MedicionNutricional.Edad_Meses_Medicion = 0;
                t_MedicionNutricional.Referencia_Normativa = REFERENCIA_OMS;

                t_MedicionNutricional.FechaRegistro = DateTime.Now;
                _context.Add(t_MedicionNutricional);
                await _context.SaveChangesAsync();

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC SP_ActualizarCategoriaIMC @ID_Medicion, @FechaMedicion, @FechaNacimiento, @ID_Sexo",
                    new SqlParameter("@ID_Medicion", t_MedicionNutricional.ID_Medicion),
                    new SqlParameter("@FechaMedicion", t_MedicionNutricional.FechaMedicion),
                    new SqlParameter("@FechaNacimiento", estudiante.FechaNacimiento),
                    new SqlParameter("@ID_Sexo", estudiante.ID_Sexo)
                );

                return RedirectToAction(nameof(Index));
            }

            PopulateDropdowns(t_MedicionNutricional);
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
            var t_MedicionNutricional = await _context.T_MedicionNutricional.FindAsync(id);
            if (t_MedicionNutricional == null) return NotFound();
            PopulateDropdowns(t_MedicionNutricional);
            return View(t_MedicionNutricional);
        }

        // POST: T_MedicionNutricional/Edit/5 - SOLO Admin y Profesor
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador del Sistema, Profesor / Encargado de Mediciones")]
        public async Task<IActionResult> Edit(int id, [Bind("ID_Medicion,ID_Estudiante,FechaMedicion,ID_DocenteEncargado,Peso_kg,Estatura_cm,Observaciones,FechaRegistro")] T_MedicionNutricional t_MedicionNutricional)
        {
            if (id != t_MedicionNutricional.ID_Medicion) return NotFound();

            if (ModelState.IsValid)
            {
                var estudiante = await _context.T_Estudiante.FindAsync(t_MedicionNutricional.ID_Estudiante);
                if (estudiante == null)
                {
                    ModelState.AddModelError("ID_Estudiante", "El estudiante seleccionado no es válido.");
                    PopulateDropdowns(t_MedicionNutricional);
                    return View(t_MedicionNutricional);
                }

                t_MedicionNutricional.ID_CategoriaIMC = null;
                t_MedicionNutricional.ZScore_IMC = 0.0m;
                t_MedicionNutricional.Edad_Meses_Medicion = 0;
                t_MedicionNutricional.Referencia_Normativa = REFERENCIA_OMS;

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
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!T_MedicionNutricionalExists(t_MedicionNutricional.ID_Medicion)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            PopulateDropdowns(t_MedicionNutricional);
            return View(t_MedicionNutricional);
        }

        // GET: T_MedicionNutricional (Index) - Accesible para Admin, Director y Profesor
        public async Task<IActionResult> Index()
        {
            var mediciones = await _context.T_MedicionNutricional
                .Include(m => m.Estudiante)
                .Include(m => m.CategoriaIMC)
                .Include(m => m.DocenteEncargado)
                .ToListAsync();

            return View(mediciones);
        }

        // GET: Details - Accesible para Admin, Director y Profesor
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var t_MedicionNutricional = await _context.T_MedicionNutricional.FirstOrDefaultAsync(m => m.ID_Medicion == id);
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

        private bool T_MedicionNutricionalExists(int id)
        {
            return _context.T_MedicionNutricional.Any(e => e.ID_Medicion == id);
        }
    }
}