using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrivenIt.Foundation.Infrastructure.Identity;
using DrivenIt.Foundation.Sample.DataLayer.Models;

namespace DrivenIt.Foundation.Sample.DataLayer
{
    public class DataContext:BaseDataContext
    {
        public DbSet<SomeBitOfData> SomeBitOfDatas { get; set; } 
    }
}
