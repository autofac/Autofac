using System;
using System.Web.Mvc;
using AttributedExample.MvcApplication.Models;

namespace AttributedExample.MvcApplication.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        private readonly Lazy<IHomeModel> _homeModel;

        public HomeController(Lazy<IHomeModel> homeModel)
        {
            _homeModel = homeModel;    
        }

        public ActionResult Index()
        {
            ViewData["Message"] = "Welcome to ASP.NET MVC!";

            return View(_homeModel.Value);
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
