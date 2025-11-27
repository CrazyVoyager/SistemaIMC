using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaIMC.Data;
using SistemaIMC.Models;
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

        public async Task<IActionResult> Index()
        {
            var estudiantesConRelaciones = await _context.T_Estudiante
                    .Include(e => e.Establecimiento)
                    .Include(e => e.Curso)
                    .Include(e => e.Sexo)
                    .ToListAsync();
            return View(estudiantesConRelaciones);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var t_Estudiante = await _context.T_Estudiante.FirstOrDefaultAsync(m => m.ID_Estudiante == id);
            if (t_Estudiante == null) return NotFound();
            return View(t_Estudiante);
        }

        // CREATE - Solo Admin y Profesor
        [Authorize(Roles = "Administrador del Sistema, Profesor / Encargado de Mediciones")]
        public async Task<IActionResult> Create()
        {
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

        private bool T_EstudianteExists(int id) => _context.T_Estudiante.Any(e => e.ID_Estudiante == id);

        [HttpGet]
        public async Task<JsonResult> GetCursosByEstablecimiento(int idEstablecimiento)
        {
            var cursos = await _context.T_Curso.Where(c => c.ID_Establecimiento == idEstablecimiento).OrderBy(c => c.NombreCurso).Select(c => new { id = c.ID_Curso, name = c.NombreCurso }).ToListAsync();
            return Json(cursos);
        }
    }
}