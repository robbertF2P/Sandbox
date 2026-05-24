using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Module.One.Datalayer;

namespace Module.One.Site.Controllers
{
    public class HomeController : Controller
    {
       
        public ActionResult Index()
        {
            var repo = new ReadOnlyRepository();
            var contract = repo.GetById();
            return View();
        }

    }
}
