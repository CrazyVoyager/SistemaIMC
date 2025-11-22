using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaIMC.Models
{
    [Table("T_MedicionNutricional")]
    public class T_MedicionNutricional
    {
        [Key]
        public int ID_Medicion { get; set; }
        public int ID_Estudiante { get; set; }
        public DateTime FechaMedicion { get; set; }
        public int ID_DocenteEncargado { get; set; }

        [Required]
        public decimal Peso_kg { get; set; }

        [Required]
        public decimal Estatura_cm { get; set; }

        // La base de datos establece ID_CategoriaIMC como NULL hasta que el SP lo actualiza
        public int? ID_CategoriaIMC { get; set; }

        // Columnas actualizadas por el SP
        public decimal ZScore_IMC { get; set; }
        public int Edad_Meses_Medicion { get; set; }
        public string Referencia_Normativa { get; set; }

        // Otros campos
        public string? Observaciones { get; set; }
        public DateTime? FechaRegistro { get; set; }

        // Propiedad IMC: Aunque la DB tiene una columna persistida,
        // mantenemos esta propiedad [NotMapped] para calcular el valor 
        // y pasarlo al SP en el controlador.
        [NotMapped]
        [Display(Name = "IMC")]
        public decimal IMC
        {
            get
            {
                if (Estatura_cm > 0)
                {
                    // Convertir Estatura de cm a metros
                    decimal estaturaMetros = Estatura_cm / 100.0m;
                    // Fórmula IMC: Peso (kg) / Estatura^2 (m^2)
                    return Peso_kg / (estaturaMetros * estaturaMetros);
                }
                return 0.0m; // Evitar división por cero
            }
        }
    }
}