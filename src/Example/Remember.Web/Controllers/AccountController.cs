using System.Web.Mvc;
using Remember.Web.Models;

namespace Remember.Web.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginForm model)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction("LoginSuccess");
            }

            
            return View(model);
        }

        
        public ActionResult LoginSuccess()
        {
            return View();
        }
    }
}