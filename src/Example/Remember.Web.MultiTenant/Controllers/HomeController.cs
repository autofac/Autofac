using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Remember.Web.Multitenant.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        readonly string _message;

        public HomeController(string message)
        {
            if (message == null) throw new ArgumentNullException("message");
            _message = message;
        }

        public ActionResult Index()
        {
            ViewData["Message"] = _message;

            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
