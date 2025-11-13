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
    public class T_CategoriaIMCController : Controller
    {
        private readonly TdDbContext _context;

        public T_CategoriaIMCController(TdDbContext context)
        {
            _context = context;
        }

        // GET: T_CategoriaIMC
        public async Task<IActionResult> Index()
        {
            return View(await _context.T_CategoriaIMCs.ToListAsync());
        }

        // GET: T_CategoriaIMC/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_CategoriaIMC = await _context.T_CategoriaIMCs
                .FirstOrDefaultAsync(m => m.ID_CategoriaIMC == id);
            if (t_CategoriaIMC == null)
            {
                return NotFound();
            }

            return View(t_CategoriaIMC);
        }

        // GET: T_CategoriaIMC/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: T_CategoriaIMC/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID_CategoriaIMC,NombreCategoria,RangoMinIMC,RangoMaxIMC")] T_CategoriaIMC t_CategoriaIMC)
        {
            if (ModelState.IsValid)
            {
                _context.Add(t_CategoriaIMC);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(t_CategoriaIMC);
        }

        // GET: T_CategoriaIMC/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_CategoriaIMC = await _context.T_CategoriaIMCs.FindAsync(id);
            if (t_CategoriaIMC == null)
            {
                return NotFound();
            }
            return View(t_CategoriaIMC);
        }

        // POST: T_CategoriaIMC/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID_CategoriaIMC,NombreCategoria,RangoMinIMC,RangoMaxIMC")] T_CategoriaIMC t_CategoriaIMC)
        {
            if (id != t_CategoriaIMC.ID_CategoriaIMC)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(t_CategoriaIMC);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!T_CategoriaIMCExists(t_CategoriaIMC.ID_CategoriaIMC))
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
            return View(t_CategoriaIMC);
        }

        // GET: T_CategoriaIMC/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_CategoriaIMC = await _context.T_CategoriaIMCs
                .FirstOrDefaultAsync(m => m.ID_CategoriaIMC == id);
            if (t_CategoriaIMC == null)
            {
                return NotFound();
            }

            return View(t_CategoriaIMC);
        }

        // POST: T_CategoriaIMC/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var t_CategoriaIMC = await _context.T_CategoriaIMCs.FindAsync(id);
            if (t_CategoriaIMC != null)
            {
                _context.T_CategoriaIMCs.Remove(t_CategoriaIMC);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool T_CategoriaIMCExists(int id)
        {
            return _context.T_CategoriaIMCs.Any(e => e.ID_CategoriaIMC == id);
        }
    }
}
