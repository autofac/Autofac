using System;
using System.Web;
using System.Web.Mvc;
using MvcShareTrader.Models;

namespace MvcShareTrader.Controllers
{
    public class HomeController : Controller
    {
        Portfolio Portfolio { get; set; }

        public HomeController(Portfolio portfolio)
        {
            if (portfolio == null)
                throw new ArgumentNullException("portfolio");

            Portfolio = portfolio;
        }

        [ControllerAction]
        public void Index()
        {
            Portfolio.Add("GNU", 1200);
            Portfolio.Add("MONO", 300);
            Portfolio.Add("LINUX", 500);
            
            RenderView("Index", Portfolio.Value);
        }
    }
}
