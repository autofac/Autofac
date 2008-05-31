using System;
using System.Linq;
using System.Web.Mvc;
using Remember.Model;
using Remember.Web.PresentationModel;

namespace Remember.Web.Controllers
{
    public class TaskController : Controller
    {
        IQueryable<Task> _tasks;

        public TaskController(IQueryable<Task> tasks)
        {
            if (tasks == null)
                throw new ArgumentNullException("tasks");

            _tasks = tasks;
        }

        public ActionResult Index()
        {
            return View("Index", new TaskList(_tasks.ToList()));
        }
    }
}
