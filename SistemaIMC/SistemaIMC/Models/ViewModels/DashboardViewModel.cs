namespace SistemaIMC.Models.ViewModels
{
    /// <summary>
    /// ViewModel para el Dashboard con estadísticas y datos para gráficos
    /// </summary>
    public class DashboardViewModel
    {
        // Contadores generales
        public int TotalEstudiantes { get; set; }
        public int TotalMediciones { get; set; }
        public int TotalEstablecimientos { get; set; }
        public int MedicionesEsteMes { get; set; }

        // Datos para gráfico de distribución por categoría IMC
        public List<CategoriaIMCEstadistica> DistribucionIMC { get; set; } = [];

        // Datos para gráfico de mediciones por mes
        public List<MedicionesPorMes> MedicionesMensuales { get; set; } = [];

        // Datos para gráfico de estudiantes por sexo
        public int EstudiantesMasculino { get; set; }
        public int EstudiantesFemenino { get; set; }

        // Datos para gráfico de mediciones por establecimiento (top 5)
        public List<EstablecimientoEstadistica> TopEstablecimientos { get; set; } = [];
    }

    public class CategoriaIMCEstadistica
    {
        public string Categoria { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class MedicionesPorMes
    {
        public string Mes { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }

    public class EstablecimientoEstadistica
    {
        public string Nombre { get; set; } = string.Empty;
        public int CantidadMediciones { get; set; }
    }
}
