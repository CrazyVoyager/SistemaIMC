using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaIMC.Models
{

    [Table("T_Sexo")]
    public class T_Sexo
    {
        [Key]
        public int ID_Sexo { get; set; }
        public string Sexo { get; set; } = string.Empty;
    }
}
