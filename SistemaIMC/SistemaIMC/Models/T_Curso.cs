using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaIMC.Models
{
    [Table("T_Curso")]
    public class T_Curso
    {
        [Key]
        public int ID_Curso { get; set; }
        public string NombreCurso { get; set; } = string.Empty;
        public int ID_Establecimiento { get; set; }

        // Propiedad de navegación
        [ForeignKey("ID_Establecimiento")]
        public T_Establecimiento? Establecimiento { get; set; }
    }
}