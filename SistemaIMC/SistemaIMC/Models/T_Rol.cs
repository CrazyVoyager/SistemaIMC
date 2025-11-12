using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaIMC.Models
{
    [Table("T_Rol")]
    public class T_Rol
    {
        [Key]
        public int ID_Rol { get; set; }
        public string NombreRol { get; set; }
    }
}
