using System;

namespace Module.One.Datalayer
{
    public class Currency
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Rate { get; set; }
    }
}