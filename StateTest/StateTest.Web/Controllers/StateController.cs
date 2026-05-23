using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StateTest.Application;
using StateTest.Web.Models;

namespace StateTest.Web.Controllers
{
    public class StateController : Controller
    {
        private readonly ControllerService _controllerService;
        //
        // GET: /State/
        public StateController(ControllerService controllerService)
        {
            _controllerService = controllerService;
        }

        public StateController():this(new ControllerService())
        {}

        public ActionResult Index()
        {
            var items = _controllerService.LoadAll();
            return View(items);
        }

        public ActionResult Detail(Guid id)
        {

            return RedirectToAction("Index");
        }

        public ActionResult Remove(Guid id)
        {
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Save(SomeViewModel viewModel)
        {
            _controllerService.Save(viewModel);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Cancel(Guid id)
        {
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(Guid id)
        {
            var vm = _controllerService.Edit(id);
            return View(vm);
        }

        [HttpPost]
        public ActionResult Reload(SomeViewModel viewModel)
        {
            ModelState.Clear();
            var vm = _controllerService.Update(viewModel);
            return PartialView("~/Views/State/EditorTemplates/SomeViewModel.cshtml", vm);
            
        }
    }

    // <summary>
    /// Controller extension class that adds controller methods
    /// to render a partial view and return the result as string.
    ///
    /// Based on http://craftycodeblog.com/2010/05/15/asp-net-mvc-render-partial-view-to-string/
    /// </summary>
    public static class ControllerExtension
    {

        /// <summary>
        /// Renders a (partial) view to string.
        /// </summary>
        /// <param name="controller">Controller to extend</param>
        /// <param name="viewName">(Partial) view to render</param>
        /// <returns>Rendered (partial) view as string</returns>
        public static string RenderPartialViewToString(this Controller controller, string viewName)
        {
            return controller.RenderPartialViewToString(viewName, null);
        }

        /// <summary>
        /// Renders a (partial) view to string.
        /// </summary>
        /// <param name="controller">Controller to extend</param>
        /// <param name="viewName">(Partial) view to render</param>
        /// <param name="model">Model</param>
        /// <returns>Rendered (partial) view as string</returns>
        public static string RenderPartialViewToString(this Controller controller, string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = controller.ControllerContext.RouteData.GetRequiredString("action");

            controller.ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }

    }
}
