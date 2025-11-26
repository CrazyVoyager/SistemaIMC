using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaIMC.Models
{
    [Table("T_Comuna")]
    public class T_Comuna
    {
        [Key]
        public int ID_Comuna { get; set; }
        public string NombreComuna { get; set; }
        public int ID_Region { get; set; }

        // Propiedad de navegación
        [ForeignKey("ID_Region")]
        public T_Region? Region { get; set; }
    }
}