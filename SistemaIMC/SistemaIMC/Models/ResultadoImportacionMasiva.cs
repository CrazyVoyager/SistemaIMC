namespace SistemaIMC.Models
{
    // Resultado simple de una importación masiva
    public class ResultadoImportacionMasiva
    {
        public int Creados { get; set; } = 0;
        public int Actualizados { get; set; } = 0;
        public List<string> Errores { get; set; } = new List<string>();
    }
}