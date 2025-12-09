using Microsoft.AspNetCore.Mvc;
using SistemaIMC.Data;
using SistemaIMC.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Linq;

namespace SistemaIMC.Controllers
{
    public class LoginController : Controller
    {
        private readonly TdDbContext _context;

        public LoginController(TdDbContext context)
        {
            _context = context;
        }

        // GET: /Login
        public IActionResult Login()
        {
            // Devuelve la vista con el formulario de inicio de sesión
            return View();
        }

        // POST: /Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string correoElectronico, string contrasena, string? returnUrl)
        {
            if (string.IsNullOrEmpty(correoElectronico) || string.IsNullOrEmpty(contrasena))
            {
                ModelState.AddModelError("", "Por favor, ingrese correo y contraseña.");
                return View();
            }

            // 1. Buscar el usuario por correo electrónico y estado activo
            // NOTA: En un sistema real, la contraseña debería estar hasheada.
            var usuario = await _context.T_Usuario
                .Include(u => u.Rol) // Necesitamos cargar el Rol para obtener el NombreRol
                .FirstOrDefaultAsync(u =>
                    u.CorreoElectronico == correoElectronico &&
                    u.Contrasena == contrasena &&
                    u.EstadoRegistro == true);

            if (usuario == null)
            {
                ModelState.AddModelError("", "Credenciales incorrectas o usuario inactivo.");
                return View();
            }

            // 2. Crear las Claims (Identidad del usuario)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.ID_Usuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.CorreoElectronico),
                // Asignamos el nombre del rol como el Claim de Rol
                new Claim(ClaimTypes.Role, usuario.Rol?.NombreRol ?? "UsuarioGeneral"),
                // Agregamos el ID del establecimiento como claim para filtrar datos por liceo
                new Claim("ID_Establecimiento", usuario.ID_Establecimiento?.ToString() ?? "")
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                // Puedes configurar propiedades de la cookie (ej. IsPersistent = true para 'Recordarme')
                IsPersistent = true,
                ExpiresUtc = System.DateTimeOffset.UtcNow.AddMinutes(30)
            };

            // 3. Iniciar Sesión (Crear la Cookie de Autenticación)
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // 4. Redireccionar
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Redirección por defecto a la página principal después del login
            return RedirectToAction("Index", "Home");
        }

        // POST: /Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Cierra la sesión (elimina la cookie)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirige a la página de login
            return RedirectToAction("Login", "Login");
        }

        // Acción para manejar accesos no autorizados (opcional, pero buena práctica)
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}