using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StateTest.Web.Models
{
    public class SomeViewModel
    {
        public string State { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public IList<SubItemViewModel> SubItems { get; set; }
    }
    public class SubItemViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
    }
}