using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datalayer.One.Models
{
    public class Relation
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ICollection<Contract> Contracts { get; set; }
    }
}
