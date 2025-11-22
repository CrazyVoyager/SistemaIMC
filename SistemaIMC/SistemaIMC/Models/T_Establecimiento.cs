using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaIMC.Models
{
    [Table("T_Establecimiento")]
    public class T_Establecimiento
    {
        [Key]
        public int ID_Establecimiento { get; set; }
        public string NombreEstablecimiento { get; set; }
        public string Direccion { get; set; }
        public int ID_Comuna { get; set; }
        public bool EstadoRegistro { get; set; }

        // Propiedad de navegación
        [ForeignKey("ID_Establecimiento")]
        public T_Establecimiento? Establecimiento { get; set; }
    }
}