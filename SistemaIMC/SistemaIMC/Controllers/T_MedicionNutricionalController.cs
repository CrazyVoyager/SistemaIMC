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
    public class T_MedicionNutricionalController : Controller
    {
        private readonly TdDbContext _context;

        public T_MedicionNutricionalController(TdDbContext context)
        {
            _context = context;
        }

        // GET: T_MedicionNutricional
        public async Task<IActionResult> Index()
        {
            return View(await _context.T_MedicionNutricional.ToListAsync());
        }

        // GET: T_MedicionNutricional/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_MedicionNutricional = await _context.T_MedicionNutricional
                .FirstOrDefaultAsync(m => m.ID_Medicion == id);
            if (t_MedicionNutricional == null)
            {
                return NotFound();
            }

            return View(t_MedicionNutricional);
        }

        // GET: T_MedicionNutricional/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: T_MedicionNutricional/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID_Medicion,ID_Estudiante,FechaMedicion,ID_DocenteEncargado,Peso_kg,IMC,ID_CategoriaIMC,Observaciones,FechaRegistro")] T_MedicionNutricional t_MedicionNutricional)
        {
            if (ModelState.IsValid)
            {
                _context.Add(t_MedicionNutricional);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(t_MedicionNutricional);
        }

        // GET: T_MedicionNutricional/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_MedicionNutricional = await _context.T_MedicionNutricional.FindAsync(id);
            if (t_MedicionNutricional == null)
            {
                return NotFound();
            }
            return View(t_MedicionNutricional);
        }

        // POST: T_MedicionNutricional/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID_Medicion,ID_Estudiante,FechaMedicion,ID_DocenteEncargado,Peso_kg,IMC,ID_CategoriaIMC,Observaciones,FechaRegistro")] T_MedicionNutricional t_MedicionNutricional)
        {
            if (id != t_MedicionNutricional.ID_Medicion)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(t_MedicionNutricional);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!T_MedicionNutricionalExists(t_MedicionNutricional.ID_Medicion))
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
            return View(t_MedicionNutricional);
        }

        // GET: T_MedicionNutricional/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_MedicionNutricional = await _context.T_MedicionNutricional
                .FirstOrDefaultAsync(m => m.ID_Medicion == id);
            if (t_MedicionNutricional == null)
            {
                return NotFound();
            }

            return View(t_MedicionNutricional);
        }

        // POST: T_MedicionNutricional/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var t_MedicionNutricional = await _context.T_MedicionNutricional.FindAsync(id);
            if (t_MedicionNutricional != null)
            {
                _context.T_MedicionNutricional.Remove(t_MedicionNutricional);
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
