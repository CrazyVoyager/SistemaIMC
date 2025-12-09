using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaIMC.Models
{
    [Table("T_Region")]
    public class T_Region
    {
        [Key]
        public int ID_Region { get; set; }
        public string NombreRegion { get; set; } = string.Empty;

        // Propiedad de navegación
        [ForeignKey("ID_Region")]
        public T_Region? Region { get; set; }

    }
}
