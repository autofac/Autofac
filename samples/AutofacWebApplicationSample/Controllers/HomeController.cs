using AutofacTestWebApplication.Services;
using Microsoft.AspNet.Mvc;

namespace AutofacTestWebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger _logger;

        public HomeController(ILogger logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            _logger.Log("Executing Index action");

            return View();
        }

        public IActionResult About()
        {
            _logger.Log("Executing About action");

            ViewBag.Message = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            _logger.Log("Executing Contact action");

            ViewBag.Message = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            _logger.Log("Executing Error action");

            return View("~/Views/Shared/Error.cshtml");
        }
    }
}