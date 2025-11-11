namespace SistemaIMC.Models
{
    public class T_Estudiante
    {

        public  int ID_Estudiante { get; set; }
        public string NombreCompleto { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Sexo { get; set; }
        public int ID_Establecimiento { get; set; }
        public int ID_Curso { get; set; }
        public bool EstadoRegistro { get; set; }
    }
}
