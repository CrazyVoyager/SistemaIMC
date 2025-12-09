using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaIMC.Models
{
    [Table("T_Usuario")]
    public class T_Usuario
    {
        [Key]
        public int ID_Usuario { get; set; }
        public string RUT { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public string? Contrasena { get; set; }
        public int ID_Rol { get; set; } 
        public bool EstadoRegistro { get; set; }
        
        // ID del establecimiento al que pertenece el usuario (nullable para Administradores del Sistema)
        public int? ID_Establecimiento { get; set; }

        // Propiedad de navegación
        [ForeignKey("ID_Rol")]
        public T_Rol? Rol { get; set; }
        
        // Propiedad de navegación al establecimiento
        [ForeignKey("ID_Establecimiento")]
        public T_Establecimiento? Establecimiento { get; set; }
    }
}
