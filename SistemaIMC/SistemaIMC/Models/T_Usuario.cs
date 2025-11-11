namespace SistemaIMC.Models
{
    public class T_Usuario
    {
        public int ID_Uusario { get; set; }
        public string RUT { get; set; }
        public string Nombre { get; set; }
        public string CorreoElectronico { get; set; }
        public string Contrasena { get; set; }
        public int ID_Rol { get; set; } 
        public bool EstadoRegistro { get; set; }
    }
}
