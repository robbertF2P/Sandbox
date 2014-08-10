using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datalayer.One.Models;

namespace Datalayer.One
{
    public class ModuleOneContext:DbContext
    {
        
        public ModuleOneContext(): base("FullContext")
        {
            Database.SetInitializer<ModuleOneContext>(null);
        }
    
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Relation> Relations { get; set; }
    }
}
