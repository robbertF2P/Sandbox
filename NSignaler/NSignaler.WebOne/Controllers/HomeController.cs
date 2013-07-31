using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using NSignaler.Contracts.UI.Commands;

namespace NSignaler.WebOne.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            DotNet.Highcharts.Highcharts chart = new DotNet.Highcharts.Highcharts("chart")
            .SetXAxis(new XAxis
            {
                Categories = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" }
            })
            .SetSeries(new Series
            {
                Data = new Data(new object[] { 29.9, 71.5, 106.4, 109.2, 144.0, 176.0, 135.6, 148.5, 216.4, 194.1, 95.6, 54.4 })
            });

            return View(chart);
        }
         
        public ActionResult Send()
        {
            MvcApplication.Bus.Send("NSignaler.Processor",
                                    new DoSomething() {Source = "WebOne", Message = "Weer een lastige klant hier"});
            return RedirectToAction("Index");
        }
    }
}
