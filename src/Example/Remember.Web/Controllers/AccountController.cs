using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Remember.Persistence;
using Remember.Model;
using Remember.Web.Models;

namespace Remember.Web.Controllers
{
    public class AccountController : Controller
    {
        private IRepository<Task> _tasks;

        public AccountController(IRepository<Task> tasks)
        {
            if (tasks == null)
                throw new ArgumentNullException("tasks");

            _tasks = tasks;
        }

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