using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Api.Core.Controllers
{
    [RoutePrefix(RoutePrefix)]
    public class TransactionProposalsController:BaseApiController
    {
        public const string RoutePrefix = "documents";
    }
}
