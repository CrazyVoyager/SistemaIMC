using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient; // NECESARIO para usar SqlParameter
using SistemaIMC.Data;
using SistemaIMC.Models;

namespace SistemaIMC.Controllers
{
    public class T_MedicionNutricionalController : Controller
    {
        private readonly TdDbContext _context;
        private const int DOCENTE_ROL_ID = 2;
        private const string REFERENCIA_OMS = "Patrones OMS 5-19 años";

        public T_MedicionNutricionalController(TdDbContext context)
        {
            _context = context;
        }

        // --------------------------------------------------------------------------------
        // MÉTODO AUXILIAR PARA CARGAR LISTAS (ViewBag)
        // --------------------------------------------------------------------------------
        private void PopulateDropdowns(T_MedicionNutricional? medicion = null)
        {
            // 1. Cargar Estudiantes
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

            // 2. Cargar Docentes Encargados
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
        // --------------------------------------------------------------------------------

        // GET: T_MedicionNutricional/Create
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }

        // POST: T_MedicionNutricional/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID_Estudiante,FechaMedicion,ID_DocenteEncargado,Peso_kg,Estatura_cm,Observaciones")] T_MedicionNutricional t_MedicionNutricional)
        {
            if (ModelState.IsValid)
            {
                // 1. Obtener datos clave del estudiante para el cálculo SP
                var estudiante = await _context.T_Estudiante.FindAsync(t_MedicionNutricional.ID_Estudiante);
                if (estudiante == null)
                {
                    ModelState.AddModelError("ID_Estudiante", "El estudiante seleccionado no es válido.");
                    PopulateDropdowns(t_MedicionNutricional);
                    return View(t_MedicionNutricional);
                }

                // 2. Preparar el registro: El SP se encarga de ZScore_IMC, Edad_Meses_Medicion y ID_CategoriaIMC.
                t_MedicionNutricional.ID_CategoriaIMC = null; // Inicia nulo o según tu configuración de DB
                t_MedicionNutricional.ZScore_IMC = 0.0m;
                t_MedicionNutricional.Edad_Meses_Medicion = 0;
                t_MedicionNutricional.Referencia_Normativa = REFERENCIA_OMS;

                // 3. Asignar FechaRegistro y guardar (esto genera ID_Medicion)
                t_MedicionNutricional.FechaRegistro = DateTime.Now;
                _context.Add(t_MedicionNutricional);
                await _context.SaveChangesAsync();

                // 4. LLAMAR AL STORED PROCEDURE para calcular y actualizar
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


        // Nuevo Método: GET /T_MedicionNutricional/GetFechaNacimiento
        [HttpGet]
        public async Task<IActionResult> GetFechaNacimiento(int idEstudiante)
        {
            var estudiante = await _context.T_Estudiante
                .Where(e => e.ID_Estudiante == idEstudiante)
                .Select(e => new { e.FechaNacimiento })
                .FirstOrDefaultAsync();

            if (estudiante == null)
            {
                return NotFound();
            }

            // Devolvemos la fecha formateada como string (YYYY-MM-DD) para el input type="date"
            return Json(new { fechaNacimiento = estudiante.FechaNacimiento.ToString("yyyy-MM-dd") });
        }

        // POST: T_MedicionNutricional/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID_Medicion,ID_Estudiante,FechaMedicion,ID_DocenteEncargado,Peso_kg,Estatura_cm,Observaciones,FechaRegistro")] T_MedicionNutricional t_MedicionNutricional)
        {
            if (id != t_MedicionNutricional.ID_Medicion) return NotFound();

            if (ModelState.IsValid)
            {
                // 1. Obtener datos clave del estudiante
                var estudiante = await _context.T_Estudiante.FindAsync(t_MedicionNutricional.ID_Estudiante);
                if (estudiante == null)
                {
                    ModelState.AddModelError("ID_Estudiante", "El estudiante seleccionado no es válido.");
                    PopulateDropdowns(t_MedicionNutricional);
                    return View(t_MedicionNutricional);
                }

                // 2. Preparar el registro para la actualización
                t_MedicionNutricional.ID_CategoriaIMC = null; // Será recalculado por el SP
                t_MedicionNutricional.ZScore_IMC = 0.0m;
                t_MedicionNutricional.Edad_Meses_Medicion = 0;
                t_MedicionNutricional.Referencia_Normativa = REFERENCIA_OMS;

                try
                {
                    _context.Update(t_MedicionNutricional);
                    await _context.SaveChangesAsync();

                    // 3. LLAMAR AL STORED PROCEDURE para recalcular y actualizar
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

        // GET: T_MedicionNutricional
        public async Task<IActionResult> Index()
        {
            // AÑADIR .Include() para cargar las propiedades de navegación
            var mediciones = await _context.T_MedicionNutricional
                .Include(m => m.Estudiante)           // Carga los datos de T_Estudiante
                .Include(m => m.CategoriaIMC)         // Carga los datos de T_CategoriaIMC
                .Include(m => m.DocenteEncargado)     // Carga los datos de T_Usuario (Docente)
                .ToListAsync();

            return View(mediciones);
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var t_MedicionNutricional = await _context.T_MedicionNutricional.FirstOrDefaultAsync(m => m.ID_Medicion == id);
            if (t_MedicionNutricional == null) return NotFound();
            return View(t_MedicionNutricional);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var t_MedicionNutricional = await _context.T_MedicionNutricional.FindAsync(id);
            if (t_MedicionNutricional == null) return NotFound();
            PopulateDropdowns(t_MedicionNutricional);
            return View(t_MedicionNutricional);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var t_MedicionNutricional = await _context.T_MedicionNutricional.FirstOrDefaultAsync(m => m.ID_Medicion == id);
            if (t_MedicionNutricional == null) return NotFound();
            return View(t_MedicionNutricional);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var t_MedicionNutricional = await _context.T_MedicionNutricional.FindAsync(id);
            if (t_MedicionNutricional != null)
            {
                _context.T_MedicionNutricional.Remove(t_MedicionNutricional);
                // Opcional: Eliminar T_Clasificacion_Nutricional (aunque ON DELETE CASCADE debería hacerlo)
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