using ContextTests.Dal.Model;
using System.Data.Entity;

namespace ContextTests.Dal
{
    public class DataContext:DbContext
    {
        public static string Connection { get; set; }
        public DataContext()
            : base((string.IsNullOrEmpty(Connection) ? "name=DataContext" : Connection))
        {
           // Configuration.LazyLoadingEnabled = false;
            //Configuration.ProxyCreationEnabled = false;
        }

        public DbSet<Student> Student { get; set; }
        public DbSet<Assignment> Assignment { get; set; }
        
    }
}
