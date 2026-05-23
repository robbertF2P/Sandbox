using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrivenIt.Foundation.Infrastructure.Data;

namespace DrivenIt.Foundation.Sample.DataLayer.Models
{
    public class SomeBitOfData : IDataModel
    {
        public SomeBitOfData()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public string MyName { get; set; }
    }
}
