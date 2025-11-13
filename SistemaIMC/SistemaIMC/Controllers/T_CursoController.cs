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
    public class T_CursoController : Controller
    {
        private readonly TdDbContext _context;

        public T_CursoController(TdDbContext context)
        {
            _context = context;
        }

        // GET: T_Curso
        public async Task<IActionResult> Index()
        {
            return View(await _context.T_Cursos.ToListAsync());
        }

        // GET: T_Curso/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_Curso = await _context.T_Cursos
                .FirstOrDefaultAsync(m => m.ID_Curso == id);
            if (t_Curso == null)
            {
                return NotFound();
            }

            return View(t_Curso);
        }

        // GET: T_Curso/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: T_Curso/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID_Curso,NombreCurso,ID_Establecimiento")] T_Curso t_Curso)
        {
            if (ModelState.IsValid)
            {
                _context.Add(t_Curso);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(t_Curso);
        }

        // GET: T_Curso/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_Curso = await _context.T_Cursos.FindAsync(id);
            if (t_Curso == null)
            {
                return NotFound();
            }
            return View(t_Curso);
        }

        // POST: T_Curso/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID_Curso,NombreCurso,ID_Establecimiento")] T_Curso t_Curso)
        {
            if (id != t_Curso.ID_Curso)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(t_Curso);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!T_CursoExists(t_Curso.ID_Curso))
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
            return View(t_Curso);
        }

        // GET: T_Curso/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_Curso = await _context.T_Cursos
                .FirstOrDefaultAsync(m => m.ID_Curso == id);
            if (t_Curso == null)
            {
                return NotFound();
            }

            return View(t_Curso);
        }

        // POST: T_Curso/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var t_Curso = await _context.T_Cursos.FindAsync(id);
            if (t_Curso != null)
            {
                _context.T_Cursos.Remove(t_Curso);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool T_CursoExists(int id)
        {
            return _context.T_Cursos.Any(e => e.ID_Curso == id);
        }
    }
}
