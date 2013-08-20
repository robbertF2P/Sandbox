using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLayer.Models;

namespace Datalayer.One
{
    public class ModuleOneContext:DbContext
    {
        public DbSet<Contract> Contracts { get; set; }
    }
}
