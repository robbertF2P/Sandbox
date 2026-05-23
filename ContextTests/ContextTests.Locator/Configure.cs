using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContextTests.Contracts.Model;

namespace ContextTests.Locator
{
    public static class  Configure
    {
        public static void Do()
        {
            AutoMapper.Mapper.CreateMap<Dal.Model.Student, Student>();
            AutoMapper.Mapper.CreateMap<Dal.Model.Assignment, Assignment>();
        }
    }
}
