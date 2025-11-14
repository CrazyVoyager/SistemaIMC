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
    public class T_UsuarioController : Controller
    {
        private readonly TdDbContext _context;

        public T_UsuarioController(TdDbContext context)
        {
            _context = context;
        }

        // GET: T_Usuario
        public async Task<IActionResult> Index()
        {
            return View(await _context.T_Usuario.ToListAsync());
        }

        // GET: T_Usuario/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_Usuario = await _context.T_Usuario
                .FirstOrDefaultAsync(m => m.ID_Usuario == id);
            if (t_Usuario == null)
            {
                return NotFound();
            }

            return View(t_Usuario);
        }

        // GET: T_Usuario/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: T_Usuario/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID_Usuario,RUT,Nombre,CorreoElectronico,Contrasena,ID_Rol,EstadoRegistro")] T_Usuario t_Usuario)
        {
            if (ModelState.IsValid)
            {
                _context.Add(t_Usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(t_Usuario);
        }

        // GET: T_Usuario/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_Usuario = await _context.T_Usuario.FindAsync(id);
            if (t_Usuario == null)
            {
                return NotFound();
            }
            return View(t_Usuario);
        }

        // POST: T_Usuario/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID_Usuario,RUT,Nombre,CorreoElectronico,Contrasena,ID_Rol,EstadoRegistro")] T_Usuario t_Usuario)
        {
            if (id != t_Usuario.ID_Usuario)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(t_Usuario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!T_UsuarioExists(t_Usuario.ID_Usuario))
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
            return View(t_Usuario);
        }

        // GET: T_Usuario/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_Usuario = await _context.T_Usuario
                .FirstOrDefaultAsync(m => m.ID_Usuario == id);
            if (t_Usuario == null)
            {
                return NotFound();
            }

            return View(t_Usuario);
        }

        // POST: T_Usuario/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var t_Usuario = await _context.T_Usuario.FindAsync(id);
            if (t_Usuario != null)
            {
                _context.T_Usuario.Remove(t_Usuario);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool T_UsuarioExists(int id)
        {
            return _context.T_Usuario.Any(e => e.ID_Usuario == id);
        }
    }
}
