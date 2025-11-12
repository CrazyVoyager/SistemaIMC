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
    public class T_RolController : Controller
    {
        private readonly TdDbContext _context;

        public T_RolController(TdDbContext context)
        {
            _context = context;
        }

        // GET: T_Rol
        public async Task<IActionResult> Index()
        {
            return View(await _context.T_Rol.ToListAsync());
        }

        // GET: T_Rol/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_Rol = await _context.T_Rol
                .FirstOrDefaultAsync(m => m.ID_Rol == id);
            if (t_Rol == null)
            {
                return NotFound();
            }

            return View(t_Rol);
        }

        // GET: T_Rol/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: T_Rol/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID_Rol,NombreRol")] T_Rol t_Rol)
        {
            if (ModelState.IsValid)
            {
                _context.Add(t_Rol);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(t_Rol);
        }

        // GET: T_Rol/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_Rol = await _context.T_Rol.FindAsync(id);
            if (t_Rol == null)
            {
                return NotFound();
            }
            return View(t_Rol);
        }

        // POST: T_Rol/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID_Rol,NombreRol")] T_Rol t_Rol)
        {
            if (id != t_Rol.ID_Rol)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(t_Rol);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!T_RolExists(t_Rol.ID_Rol))
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
            return View(t_Rol);
        }

        // GET: T_Rol/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_Rol = await _context.T_Rol
                .FirstOrDefaultAsync(m => m.ID_Rol == id);
            if (t_Rol == null)
            {
                return NotFound();
            }

            return View(t_Rol);
        }

        // POST: T_Rol/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var t_Rol = await _context.T_Rol.FindAsync(id);
            if (t_Rol != null)
            {
                _context.T_Rol.Remove(t_Rol);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool T_RolExists(int id)
        {
            return _context.T_Rol.Any(e => e.ID_Rol == id);
        }
    }
}
