using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Module.One.Datalayer
{
    [Table("Contracts")]
    public class ModuleOneContract
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}