using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Remember.Web.Controllers
{
    public class HomeController : Controller
    {
        public void Index()
        {
            RenderView("Index");
        }

        public void About()
        {
            RenderView("About");
        }
    }
}
