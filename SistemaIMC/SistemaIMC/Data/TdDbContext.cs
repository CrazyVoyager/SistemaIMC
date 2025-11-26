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

        public DbSet<T_CategoriaIMC> T_CategoriaIMCs { get; set; }
        public DbSet<T_Comuna> T_Comunas { get; set; }
        public DbSet<T_Curso> T_Curso { get; set; }
        public DbSet<T_Establecimiento> T_Establecimientos { get; set; }
        public DbSet<SistemaIMC.Models.T_Estudiante> T_Estudiante { get; set; } = default!;
        public DbSet<SistemaIMC.Models.T_MedicionNutricional> T_MedicionNutricional { get; set; } = default!;
        public DbSet<SistemaIMC.Models.T_Region> T_Region { get; set; } = default!;
        public DbSet<SistemaIMC.Models.T_Rol> T_Rol { get; set; } = default!;
        public DbSet<SistemaIMC.Models.T_Usuario> T_Usuario { get; set; } = default!;
        public DbSet<SistemaIMC.Models.T_Sexo> T_Sexo { get; set; } = default!;



    }
}
