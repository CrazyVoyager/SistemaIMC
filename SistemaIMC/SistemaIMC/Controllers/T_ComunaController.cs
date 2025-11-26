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
    public class T_ComunaController : Controller
    {
        private readonly TdDbContext _context;

        public T_ComunaController(TdDbContext context)
        {
            _context = context;
        }

        // GET: T_Comuna
        public async Task<IActionResult> Index()
        {
            var comunas = await _context.T_Comunas
                    .Include(c => c.Region)
                    .ToListAsync();

            return View(comunas);
        }

        // GET: T_Comuna/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_Comuna = await _context.T_Comunas
                .FirstOrDefaultAsync(m => m.ID_Comuna == id);
            if (t_Comuna == null)
            {
                return NotFound();
            }

            return View(t_Comuna);
        }

        // GET: T_Comuna/Create
        public async Task<IActionResult> Create()
        {
            await LoadRegionesViewBag();
            return View();
        }

        // POST: T_Comuna/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID_Comuna,NombreComuna,ID_Region")] T_Comuna t_Comuna)
        {
            if (ModelState.IsValid)
            {
                _context.Add(t_Comuna);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Ya está marcado como async, y la recarga es correcta
            await LoadRegionesViewBag(t_Comuna.ID_Region);
            return View(t_Comuna);
        }



        // GET: T_Comuna/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_Comuna = await _context.T_Comunas.FindAsync(id);
            if (t_Comuna == null)
            {
                return NotFound();
            }

            await LoadRegionesViewBag(t_Comuna.ID_Region);

            return View(t_Comuna);
        }

        // POST: T_Comuna/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID_Comuna,NombreComuna,ID_Region")] T_Comuna t_Comuna)
        {
            if (id != t_Comuna.ID_Comuna)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(t_Comuna);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!T_ComunaExists(t_Comuna.ID_Comuna))
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

            await LoadRegionesViewBag(t_Comuna.ID_Region);

            return View(t_Comuna);
        }

        // GET: T_Comuna/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_Comuna = await _context.T_Comunas
                .FirstOrDefaultAsync(m => m.ID_Comuna == id);
            if (t_Comuna == null)
            {
                return NotFound();
            }

            return View(t_Comuna);
        }

        // POST: T_Comuna/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var t_Comuna = await _context.T_Comunas.FindAsync(id);
            if (t_Comuna != null)
            {
                _context.T_Comunas.Remove(t_Comuna);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadRegionesViewBag(object selectedRegion = null)
        {
            ViewBag.ID_Region = new SelectList(
                await _context.T_Region.OrderBy(r => r.NombreRegion).ToListAsync(),
                "ID_Region",
                "NombreRegion",
                selectedRegion
            );
        }

        private bool T_ComunaExists(int id)
        {
            return _context.T_Comunas.Any(e => e.ID_Comuna == id);
        }
    }
}