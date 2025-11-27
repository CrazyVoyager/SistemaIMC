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


    [Authorize(Roles = "Administrador del Sistema")]
    public class T_RegionController : Controller
    {
        private readonly TdDbContext _context;

        public T_RegionController(TdDbContext context)
        {
            _context = context;
        }

        // GET: T_Region
        public async Task<IActionResult> Index()
        {
            return View(await _context.T_Region.ToListAsync());
        }

        // GET: T_Region/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_Region = await _context.T_Region
                .FirstOrDefaultAsync(m => m.ID_Region == id);
            if (t_Region == null)
            {
                return NotFound();
            }

            return View(t_Region);
        }

        // GET: T_Region/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: T_Region/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID_Region,NombreRegion")] T_Region t_Region)
        {
            if (ModelState.IsValid)
            {
                _context.Add(t_Region);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(t_Region);
        }

        // GET: T_Region/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_Region = await _context.T_Region.FindAsync(id);
            if (t_Region == null)
            {
                return NotFound();
            }
            return View(t_Region);
        }

        // POST: T_Region/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID_Region,NombreRegion")] T_Region t_Region)
        {
            if (id != t_Region.ID_Region)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(t_Region);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!T_RegionExists(t_Region.ID_Region))
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
            return View(t_Region);
        }

        // GET: T_Region/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_Region = await _context.T_Region
                .FirstOrDefaultAsync(m => m.ID_Region == id);
            if (t_Region == null)
            {
                return NotFound();
            }

            return View(t_Region);
        }

        // POST: T_Region/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var t_Region = await _context.T_Region.FindAsync(id);
            if (t_Region != null)
            {
                _context.T_Region.Remove(t_Region);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool T_RegionExists(int id)
        {
            return _context.T_Region.Any(e => e.ID_Region == id);
        }
    }
}
