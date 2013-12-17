using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Remember.Web.Areas.Integration.Models;

namespace Remember.Web.Areas.Integration.Controllers
{
    /// <summary>
    /// Simple integration tests for the <see cref="Autofac.Integration.Mvc.ExtensibleActionInvoker"/>
    /// </summary>
    public class InvokerController : Controller
    {
        public ActionResult FileUpload()
        {
            return View();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ParameterInjection(string value, int id, IInvokerDependency resolved, NotRegistered notRegistered)
        {
            this.ViewData["Value"] = value;
            this.ViewData["Id"] = id;

            // IInvokerDependency should be resolved by the extensible action invoker.
            if (resolved == null)
            {
                this.ViewData["Resolved"] = "was not resolved but should have been";
            }
            else
            {
                this.ViewData["Resolved"] = "was resolved by the action invoker";
            }

            // The concrete/not registered dependency should pass through model binding
            // and not be resolved by the action invoker.
            if (notRegistered == null)
            {
                this.ViewData["NotRegistered"] = "was incorrectly resolved by the action invoker";
            }
            else
            {
                this.ViewData["NotRegistered"] = "was (correctly) left as null since it wasn't passed in and isn't registered with the container";
            }

            return View();
        }

        [HttpPost]
        public ActionResult ProcessFileUploads(IEnumerable<HttpPostedFileBase> files)
        {
            return this.View(files.Where(f => f.ContentLength > 0).Select(f => f.FileName));
        }
    }
}
