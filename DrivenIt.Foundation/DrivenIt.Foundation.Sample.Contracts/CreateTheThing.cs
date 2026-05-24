using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DrivenIt.Foundation.Contracts;

namespace DrivenIt.Foundation.Sample.Contracts
{
    public class CreateTheThing:IDomainTask
    {
        public string TheNameToUse { get; set; }
    }
}
