using Microsoft.EntityFrameworkCore;

namespace SistemaIMC.Data
{
    public class TdDbContext : DbContext
    {
        public TdDbContext(DbContextOptions<TdDbContext> options)
          : base(options)
        {


        }
    }
}
