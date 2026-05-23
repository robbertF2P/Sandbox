using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Module.Two.Datalayer
{
    public class Contract
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool SomeProperty { get; set; }
    }
}
