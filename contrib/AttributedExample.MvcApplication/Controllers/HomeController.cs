using System.Web.Mvc;
using AttributedExample.MvcApplication.Models;

namespace AttributedExample.MvcApplication.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        private IHomeModel _homeModel;

        public HomeController(IHomeModel homeModel)
        {
            _homeModel = homeModel;    
        }

        public ActionResult Index()
        {
            ViewData["Message"] = "Welcome to ASP.NET MVC!";

            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
