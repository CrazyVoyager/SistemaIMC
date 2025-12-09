using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaIMC.Data;
using SistemaIMC.Models;
using SistemaIMC.Models.ViewModels;

namespace SistemaIMC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TdDbContext _context;

        public HomeController(ILogger<HomeController> logger, TdDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Obtiene el ID del establecimiento del usuario logueado desde los claims.
        /// </summary>
        private int? GetEstablecimientoUsuarioLogueado()
        {
            var establecimientoClaim = User.FindFirst("ID_Establecimiento")?.Value;
            if (!string.IsNullOrEmpty(establecimientoClaim) && int.TryParse(establecimientoClaim, out int establecimientoId))
            {
                return establecimientoId;
            }
            return null;
        }

        /// <summary>
        /// Verifica si el usuario actual es Administrador del Sistema.
        /// </summary>
        private bool EsAdministrador()
        {
            return User.IsInRole("Administrador del Sistema");
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel();
            var establecimientoUsuario = GetEstablecimientoUsuarioLogueado();
            var esAdmin = EsAdministrador();

            try
            {
                // Query base para estudiantes
                var estudiantesQuery = _context.T_Estudiante.AsQueryable();
                var medicionesQuery = _context.T_MedicionNutricional
                    .Include(m => m.Estudiante)
                    .Include(m => m.CategoriaIMC)
                    .AsQueryable();

                // Si no es admin, filtrar por establecimiento
                if (!esAdmin && establecimientoUsuario.HasValue)
                {
                    estudiantesQuery = estudiantesQuery.Where(e => e.ID_Establecimiento == establecimientoUsuario.Value);
                    medicionesQuery = medicionesQuery.Where(m => m.Estudiante != null && m.Estudiante.ID_Establecimiento == establecimientoUsuario.Value);
                }

                // Contadores generales
                viewModel.TotalEstudiantes = await estudiantesQuery.CountAsync();
                viewModel.TotalMediciones = await medicionesQuery.CountAsync();
                viewModel.TotalEstablecimientos = esAdmin 
                    ? await _context.T_Establecimientos.CountAsync() 
                    : 1;

                var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                viewModel.MedicionesEsteMes = await medicionesQuery
                    .Where(m => m.FechaMedicion >= inicioMes)
                    .CountAsync();

                // Distribución por categoría IMC
                var categoriasIMC = await medicionesQuery
                    .Where(m => m.CategoriaIMC != null)
                    .GroupBy(m => m.CategoriaIMC!.NombreCategoria)
                    .Select(g => new CategoriaIMCEstadistica
                    {
                        Categoria = g.Key,
                        Cantidad = g.Count()
                    })
                    .ToListAsync();

                // Asignar colores según categoría
                var coloresCategorias = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Bajo peso severo", "#dc3545" },
                    { "Bajo peso", "#fd7e14" },
                    { "Normal", "#28a745" },
                    { "Sobrepeso", "#ffc107" },
                    { "Obesidad", "#dc3545" },
                    { "Obesidad severa", "#6f42c1" }
                };

                foreach (var cat in categoriasIMC)
                {
                    cat.Color = coloresCategorias.GetValueOrDefault(cat.Categoria, "#6c757d");
                }
                viewModel.DistribucionIMC = categoriasIMC;

                // Mediciones por mes (últimos 6 meses)
                var hace6Meses = DateTime.Now.AddMonths(-5);
                var primerDiaMes = new DateTime(hace6Meses.Year, hace6Meses.Month, 1);

                var medicionesPorMes = await medicionesQuery
                    .Where(m => m.FechaMedicion >= primerDiaMes)
                    .GroupBy(m => new { m.FechaMedicion.Year, m.FechaMedicion.Month })
                    .Select(g => new
                    {
                        Anio = g.Key.Year,
                        Mes = g.Key.Month,
                        Cantidad = g.Count()
                    })
                    .OrderBy(x => x.Anio)
                    .ThenBy(x => x.Mes)
                    .ToListAsync();

                var nombresMeses = new[] { "", "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };
                viewModel.MedicionesMensuales = medicionesPorMes.Select(m => new MedicionesPorMes
                {
                    Mes = $"{nombresMeses[m.Mes]} {m.Anio}",
                    Cantidad = m.Cantidad
                }).ToList();

                // Estudiantes por sexo
                viewModel.EstudiantesMasculino = await estudiantesQuery.CountAsync(e => e.ID_Sexo == 1);
                viewModel.EstudiantesFemenino = await estudiantesQuery.CountAsync(e => e.ID_Sexo == 2);

                // Top 5 establecimientos con más mediciones (solo para admin)
                if (esAdmin)
                {
                    viewModel.TopEstablecimientos = await _context.T_MedicionNutricional
                        .Include(m => m.Estudiante)
                            .ThenInclude(e => e!.Establecimiento)
                        .Where(m => m.Estudiante != null && m.Estudiante.Establecimiento != null)
                        .GroupBy(m => m.Estudiante!.Establecimiento!.NombreEstablecimiento)
                        .Select(g => new EstablecimientoEstadistica
                        {
                            Nombre = g.Key,
                            CantidadMediciones = g.Count()
                        })
                        .OrderByDescending(x => x.CantidadMediciones)
                        .Take(5)
                        .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar estadísticas del dashboard");
            }

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
