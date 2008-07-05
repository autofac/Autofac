using System;
using System.Linq;
using System.Web.Mvc;
using Remember.Model;
using Remember.Persistence;

namespace Remember.Web.Controllers
{
    public class TaskController : Controller
    {
        IRepository<Task> _tasks;

        public TaskController(IRepository<Task> tasks)
        {
            if (tasks == null)
                throw new ArgumentNullException("tasks");

            _tasks = tasks;
        }

        public ActionResult Index()
        {
            var outstandingTasks = _tasks.FindBySpecification(new IncompleteTaskSpecification());
            return View("Index", outstandingTasks);
        }
    }
}
