using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EasyNetQ;
using MyBackend;

namespace MySpa.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var bus = RabbitHutch.CreateBus("host=localhost");
            var request = new RequestServerTime() {Text = "Hoi"};
            var task = bus.RequestAsync<RequestServerTime, ResponseServerTime>(request);
            task.ContinueWith(response => {
                    Console.WriteLine("Got response: '{0}'", response.Result.Text);
                });
            return View();
        }
    }
}
