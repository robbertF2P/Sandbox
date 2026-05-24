using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datalayer
{
    public class DataContext:DbContext
    {
        public DbSet<Module.Two.Datalayer.Contract> Contracts { get; set; }
        public DbSet<Module.One.Datalayer.Currency> Currencies { get; set; }
    }
}
