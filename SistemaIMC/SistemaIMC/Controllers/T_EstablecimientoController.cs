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
    public class T_EstablecimientoController : Controller
    {
        private readonly TdDbContext _context;

        public T_EstablecimientoController(TdDbContext context)
        {
            _context = context;
        }

        // GET: T_Establecimiento
        public async Task<IActionResult> Index()
        {
            var establecimiento = await _context.T_Establecimientos
                                .Include(c => c.Comuna)
                                .ToListAsync();

            return View(establecimiento);
        }

        // GET: T_Establecimiento/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_Establecimiento = await _context.T_Establecimientos
                .FirstOrDefaultAsync(m => m.ID_Establecimiento == id);
            if (t_Establecimiento == null)
            {
                return NotFound();
            }

            return View(t_Establecimiento);
        }

        // GET: T_Establecimiento/Create
        // ⭐ CAMBIO: Método asíncrono y carga del ViewBag ⭐
        public async Task<IActionResult> Create()
        {
            await LoadComunasViewBag();
            return View();
        }

        // POST: T_Establecimiento/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID_Establecimiento,NombreEstablecimiento, Direccion, ID_Comuna, EstadoRegistro")] T_Establecimiento t_Establecimiento)
        {
            // 1. Validación del Modelo (Datos del formulario)
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(t_Establecimiento);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    // 2. Captura errores específicos de Base de Datos (ej: claves duplicadas)
                    // Opcional: Loguear dbEx
                    ModelState.AddModelError("", "No se pudieron guardar los cambios. Intente nuevamente o contacte al administrador si el problema persiste.");
                }
                catch (Exception ex)
                {
                    // 3. Captura cualquier otro error general
                    // Opcional: Loguear ex
                    ModelState.AddModelError("", $"Ocurrió un error inesperado: {ex.Message}");
                }
            }

            // 4. Si llegamos aquí, hubo un error (de validación o en el try-catch).
            // Recargamos las listas desplegables (ViewBag) para que el formulario no se rompa.
            await LoadComunasViewBag(t_Establecimiento.ID_Comuna);

            // Retornamos la vista con el modelo para mostrar los errores y mantener los datos que el usuario ya escribió.
            return View(t_Establecimiento);
        }
        // GET: T_Establecimiento/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_Establecimiento = await _context.T_Establecimientos.FindAsync(id);
            if (t_Establecimiento == null)
            {
                return NotFound();
            }

            // ⭐ Se carga el ViewBag para edición ⭐
            await LoadComunasViewBag(t_Establecimiento.ID_Comuna);
            return View(t_Establecimiento);
        }

        // POST: T_Establecimiento/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID_Establecimiento,NombreEstablecimiento, Direccion,ID_Comuna, EstadoRegistro")] T_Establecimiento t_Establecimiento)
        {
            if (id != t_Establecimiento.ID_Establecimiento)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(t_Establecimiento);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!T_EstablecimientoExists(t_Establecimiento.ID_Establecimiento))
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

            // ⭐ Se recarga el ViewBag si la validación falla en Edit ⭐
            await LoadComunasViewBag(t_Establecimiento.ID_Comuna);
            return View(t_Establecimiento);
        }

        // GET: T_Establecimiento/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var t_Establecimiento = await _context.T_Establecimientos
                .FirstOrDefaultAsync(m => m.ID_Establecimiento == id);
            if (t_Establecimiento == null)
            {
                return NotFound();
            }

            return View(t_Establecimiento);
        }

        // POST: T_Establecimiento/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var t_Establecimiento = await _context.T_Establecimientos.FindAsync(id);
            if (t_Establecimiento != null)
            {
                _context.T_Establecimientos.Remove(t_Establecimiento);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ⭐ MÉTODO AUXILIAR PARA CARGAR LAS COMUNAS ⭐
        private async Task LoadComunasViewBag(object? selectedComuna = null)
        {
            // Asumo que tu contexto tiene una tabla T_Comunas
            ViewBag.ID_Comuna = new SelectList(
                await _context.T_Comunas.OrderBy(c => c.NombreComuna).ToListAsync(),
                "ID_Comuna",
                "NombreComuna",
                selectedComuna
            );
        }

        private bool T_EstablecimientoExists(int id)
        {
            return _context.T_Establecimientos.Any(e => e.ID_Establecimiento == id);
        }
    }
}