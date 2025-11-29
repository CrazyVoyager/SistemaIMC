// Ubicación: SistemaIMC/Models/T_Clasificacion_Nutricional.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaIMC.Models
{
    // Mapea a la tabla en la base de datos
    [Table("T_Clasificacion_Nutricional")]
    public class T_Clasificacion_Nutricional
    {
        [Key]
        public int ID_Clasificacion { get; set; }

        // Clave Foránea a la medición. También es la clave única (UNIQUE) en la tabla SQL.
        [Required]
        public int ID_Medicion { get; set; }

        [Required]
        public int Edad_Meses { get; set; }

        [Required]
        [Column(TypeName = "decimal(4, 2)")]
        public decimal Puntuacion_Z { get; set; }

        [Required]
        public string Categoria { get; set; }

        public string? Referencia_Normativa { get; set; }

        // Propiedad de Navegación (para la relación 1:1)
        [ForeignKey("ID_Medicion")]
        public T_MedicionNutricional? Medicion { get; set; }


        [NotMapped]
        public string ColorClasificacion
        {
            get
            {
                // Lógica para asignar el color de Bootstrap basado en la categoría.
                return Categoria switch
                {
                    "Bajo Peso" => "warning",
                    "Normal" => "success",
                    "Sobrepeso" => "warning",
                    "Obesidad" => "danger",
                    _ => "secondary", // Valor por defecto
                };
            }
        }
    }
}