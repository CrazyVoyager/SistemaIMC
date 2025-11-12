using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaIMC.Models
{
    [Table("T_Usuario")]
    public class T_Usuario
    {
        [Key]
        public int ID_Uusario { get; set; }
        public string RUT { get; set; }
        public string Nombre { get; set; }
        public string CorreoElectronico { get; set; }
        public string Contrasena { get; set; }
        public int ID_Rol { get; set; } 
        public bool EstadoRegistro { get; set; }
    }
}
