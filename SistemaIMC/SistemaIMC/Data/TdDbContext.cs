using Microsoft.EntityFrameworkCore;
using SistemaIMC.Models;

namespace SistemaIMC.Data
{
    public class TdDbContext : DbContext
    {
        public TdDbContext(DbContextOptions<TdDbContext> options)
          : base(options)
        {


        }
        public DbSet<SistemaIMC.Models.T_Estudiante> T_Estudiante { get; set; } = default!;
        public DbSet<SistemaIMC.Models.T_MedicionNutricional> T_MedicionNutricional { get; set; } = default!;
        public DbSet<SistemaIMC.Models.T_Region> T_Region { get; set; } = default!;
        public DbSet<SistemaIMC.Models.T_Rol> T_Rol { get; set; } = default!;
        public DbSet<SistemaIMC.Models.T_Usuario> T_Usuario { get; set; } = default!;
    }
}
