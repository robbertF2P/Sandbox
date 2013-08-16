using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Messages.Module.One.Commands;

namespace TheSite.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            MvcApplication.Bus.Send(new CreateContract());
            return View();
        }

    }
}
