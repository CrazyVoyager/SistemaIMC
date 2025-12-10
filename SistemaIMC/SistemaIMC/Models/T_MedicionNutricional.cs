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
        [Column(TypeName = "decimal(5,2)")]
        public decimal Peso_kg { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal Estatura_cm { get; set; }

        // La base de datos establece ID_CategoriaIMC como NULL hasta que el SP lo actualiza
        public int? ID_CategoriaIMC { get; set; }

        // Columnas actualizadas por el SP
        [Column(TypeName = "decimal(5,2)")]
        public decimal? ZScore_IMC { get; set; }
        public int? Edad_Meses_Medicion { get; set; }
        [Column(TypeName = "varchar(100)")]
        public string? Referencia_Normativa { get; set; }

        public T_Clasificacion_Nutricional? ClasificacionFinal { get; set; }

        // Otros campos
        [Column(TypeName = "nvarchar(1000)")]
        public string? Observaciones { get; set; }
        public DateTime? FechaRegistro { get; set; }

        // 1) Propiedad mapeada a la columna IMC en la BD.
        //    La dejamos con un nombre distinto en C# para evitar romper código que espera
        //    la propiedad calculada "IMC". Se mapea explícitamente al nombre de columna "IMC".
        [Column("IMC", TypeName = "numeric(38,21)")]
        public decimal? IMC_Persistido { get; set; }

        // 2) Propiedad IMC (no mappeada): mantiene la lógica actual (cálculo local),
        //    pero ahora devuelve el valor persistido si existe (prioriza IMC_Persistido).
        //    De este modo el resto de la aplicación que usa .IMC sigue funcionando y ve
        //    el valor de la BD cuando esté disponible.
        [NotMapped]
        [Display(Name = "IMC")]
        public decimal? IMC
        {
            get
            {
                // Si hay un valor persistido en BD, usarlo
                if (IMC_Persistido.HasValue)
                {
                    return IMC_Persistido.Value;
                }

                // Si no, calcular a partir de peso y estatura (comportamiento previo)
                if (Estatura_cm > 0)
                {
                    decimal estaturaMetros = Estatura_cm / 100.0m;
                    return Peso_kg / (estaturaMetros * estaturaMetros);
                }

                return null;
            }
        }

        // Propiedad de navegación
        [ForeignKey("ID_Estudiante")]
        public T_Estudiante? Estudiante { get; set; }

        // Propiedad de navegación
        [ForeignKey("ID_DocenteEncargado")]
        public T_Usuario? DocenteEncargado { get; set; }

        // Propiedad de navegación
        [ForeignKey("ID_CategoriaIMC")]
        public T_CategoriaIMC? CategoriaIMC { get; set; }
    }
}