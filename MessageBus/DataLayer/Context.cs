using System.Data.Entity;
using DataLayer.Models;

namespace DataLayer
{
    public class FullContext:DbContext
    {
        public DbSet<Contract> Contracts { get; set; } 
    }
}
