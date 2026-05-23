using System;
using System.Collections.Generic;

namespace StateTest.Domain
{
    public class SomeDomainThing
    {
        public SomeDomainThing(string name)
        {
            Name = name;
            Id = Guid.NewGuid();
        }
        private readonly List<SubItem> _subItems = new List<SubItem>();

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public IEnumerable<SubItem> SubItems {
            get { return _subItems; }
        } 
        public void AddNewItem(string name, decimal value)
        {
            _subItems.Add(new SubItem
                {
                    Id = Guid.NewGuid(), 
                    Name = name, 
                    Value = value
                });
        }

        public void ChangeName(string name)
        {
            if (name == "piet")
            {
                Name = "Piet niet";
            }
            else
            {
                Name = name.ToUpper();
            }
        }
    }

    public class SubItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
    }
}
