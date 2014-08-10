using System.Data.Entity;

namespace DataLayer
{
    public class FullContext:DbContext
    {
        public DbSet<Datalayer.One.Models.Contract> Contracts { get; set; } 
    }
}
