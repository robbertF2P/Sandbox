using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Api.Core.Controllers
{
    public abstract class BaseApiController:ApiController
    {
        protected string GetCurrentRoot()
        {
            return Request.RequestUri.AbsoluteUri.Replace(Request.RequestUri.PathAndQuery, "/");
        }
    }
}
