using System.Data.Entity;
using DataLayer.Module.One.Models;

namespace DataLayer.Module.One
{
    public class ModuleOneContext:DbContext
    {
        public DbSet<Contract> Contracts { get; set; } 
    }
}
