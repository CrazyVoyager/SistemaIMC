using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaIMC.Data;
using SistemaIMC.Models;

namespace SistemaIMC.Controllers
{
    public class T_EstudianteController : Controller
    {
        private readonly TdDbContext _context;

        public T_EstudianteController(TdDbContext context)
        {
            _context = context;
        }

        // GET: T_Estudiante
        public async Task<IActionResult> Index()
        {
            return View(await _context.T_Estudiante.ToListAsync());
        }

        // GET: T_Estudiante/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_Estudiante = await _context.T_Estudiante
                .FirstOrDefaultAsync(m => m.ID_Estudiante == id);
            if (t_Estudiante == null)
            {
                return NotFound();
            }

            return View(t_Estudiante);
        }

        // GET: T_Estudiante/Create
        public async Task<IActionResult> Create()
        {
            // Cargar Establecimientos
        ViewBag.ID_Establecimiento = new SelectList(
        await _context.T_Establecimientos.OrderBy(e => e.NombreEstablecimiento).ToListAsync(),
        "ID_Establecimiento",
        "NombreEstablecimiento"
    );

            // Cargar Cursos
            ViewBag.ID_Curso = new SelectList(
                await _context.T_Cursos.OrderBy(c => c.NombreCurso).ToListAsync(),
                "ID_Curso",
                "NombreCurso"
            );


            return View();
        }

        // POST: T_Estudiante/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        // GET: T_Estudiante/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_Estudiante = await _context.T_Estudiante.FindAsync(id);
            if (t_Estudiante == null)
            {
                return NotFound();
            }
            return View(t_Estudiante);
        }

        // POST: T_Estudiante/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID_Estudiante,RUT,NombreCompleto,FechaNacimiento,ID_Sexo,ID_Establecimiento,ID_Curso,EstadoRegistro")] T_Estudiante t_Estudiante)
        {
            if (id != t_Estudiante.ID_Estudiante)
            {
                return NotFound();
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
                    if (!T_EstudianteExists(t_Estudiante.ID_Estudiante))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(t_Estudiante);
        }

        // GET: T_Estudiante/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_Estudiante = await _context.T_Estudiante
                .FirstOrDefaultAsync(m => m.ID_Estudiante == id);
            if (t_Estudiante == null)
            {
                return NotFound();
            }

            return View(t_Estudiante);
        }

        // POST: T_Estudiante/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var t_Estudiante = await _context.T_Estudiante.FindAsync(id);
            if (t_Estudiante != null)
            {
                _context.T_Estudiante.Remove(t_Estudiante);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool T_EstudianteExists(int id)
        {
            return _context.T_Estudiante.Any(e => e.ID_Estudiante == id);
        }

        [HttpGet]
        public async Task<JsonResult> GetCursosByEstablecimiento(int idEstablecimiento)
        {
            var cursos = await _context.T_Cursos
                .Where(c => c.ID_Establecimiento == idEstablecimiento)
                .OrderBy(c => c.NombreCurso)
                .Select(c => new
                {
                    id = c.ID_Curso,
                    name = c.NombreCurso
                })
                .ToListAsync();

            return Json(cursos);
        }

    }
}
