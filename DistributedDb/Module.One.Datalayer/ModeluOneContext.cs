using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Module.One.Datalayer
{
    public class ModeluOneContext : DbContext
    {
        public ModeluOneContext()
            : base("DataContext")
        {
            Database.SetInitializer<ModeluOneContext>(null);
        }
        public DbSet<ModuleOneContract> Contracts { get; set; }
        public DbSet<Currency> Currencies { get; set; }
    }
}
