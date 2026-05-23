using System.Data.Entity;

namespace Module.Two.Datalayer
{
    public class ModeluTwoContext : DbContext
    {
        public ModeluTwoContext()
            : base("DataContext")
        {
            Database.SetInitializer<ModeluTwoContext>(null);
        }
        public DbSet<Contract> Contracts { get; set; }
    }
}
