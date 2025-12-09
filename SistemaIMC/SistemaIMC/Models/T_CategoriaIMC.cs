using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaIMC.Models
{
    [Table("T_CategoriaIMC")]
    public class T_CategoriaIMC
    {
        [Key]
        public int ID_CategoriaIMC { get; set; }
        public string NombreCategoria { get; set; } = string.Empty;

        [NotMapped]
        public decimal? RangoMinIMC { get; set; }
        [NotMapped]
        public decimal? RangoMaxIMC { get; set; }
    }
}