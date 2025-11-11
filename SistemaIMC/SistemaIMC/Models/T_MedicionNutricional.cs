namespace SistemaIMC.Models
{
    public class T_MedicionNutricional
    {
        public int ID_Medicion { get; set; }
        public int ID_Estudiante { get; set; }
        public DateTime FechaMedicion { get; set; }
        public int ID_DocenteEncargado { get; set; }
        public decimal Peso_kg { get; set; }
        public string IMC { get; set; }
        public int ID_CategoriaIMC { get; set; }
        public string Observaciones { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
