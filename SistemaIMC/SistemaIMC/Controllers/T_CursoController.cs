using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaIMC.Data;
using SistemaIMC.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaIMC.Controllers
{
    // Director puede ver
    [Authorize(Roles = "Administrador del Sistema, Supervisor / Director de la Entidad")]
    public class T_CursoController : Controller
    {
        private readonly TdDbContext _context;

        public T_CursoController(TdDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
        {
            var curso = await _context.T_Curso.Include(c => c.Establecimiento).ToListAsync();
            return View(curso);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var t_Curso = await _context.T_Curso.FirstOrDefaultAsync(m => m.ID_Curso == id);
            if (t_Curso == null) return NotFound();
            return View(t_Curso);
        }

        // CREATE - SOLO ADMIN
        [Authorize(Roles = "Administrador del Sistema")]
        public async Task<IActionResult> Create()
        {
            await LoadEstablecimientosViewBag();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador del Sistema")]
        public async Task<IActionResult> Create([Bind("ID_Curso,NombreCurso,ID_Establecimiento")] T_Curso t_Curso)
        {
            if (ModelState.IsValid)
            {
                _context.Add(t_Curso);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            await LoadEstablecimientosViewBag(t_Curso.ID_Establecimiento);
            return View(t_Curso);
        }

        // EDIT - SOLO ADMIN
        [Authorize(Roles = "Administrador del Sistema")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var t_Curso = await _context.T_Curso.FindAsync(id);
            if (t_Curso == null) return NotFound();
            return View(t_Curso);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador del Sistema")]
        public async Task<IActionResult> Edit(int id, [Bind("ID_Curso,NombreCurso,ID_Establecimiento")] T_Curso t_Curso)
        {
            if (id != t_Curso.ID_Curso) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(t_Curso);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!T_CursoExists(t_Curso.ID_Curso)) return NotFound(); else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            await LoadEstablecimientosViewBag(t_Curso.ID_Establecimiento);
            return View(t_Curso);
        }

        // DELETE - SOLO ADMIN
        [Authorize(Roles = "Administrador del Sistema")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var t_Curso = await _context.T_Curso.FirstOrDefaultAsync(m => m.ID_Curso == id);
            if (t_Curso == null) return NotFound();
            return View(t_Curso);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador del Sistema")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var t_Curso = await _context.T_Curso.FindAsync(id);
            if (t_Curso != null) _context.T_Curso.Remove(t_Curso);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadEstablecimientosViewBag(object selectedEstablecimiento = null)
        {
            ViewBag.ID_Establecimiento = new SelectList(await _context.T_Establecimientos.OrderBy(e => e.NombreEstablecimiento).ToListAsync(), "ID_Establecimiento", "NombreEstablecimiento", selectedEstablecimiento);
        }
        private bool T_CursoExists(int id) => _context.T_Curso.Any(e => e.ID_Curso == id);
    }
}