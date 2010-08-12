using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Remember.Persistence;
using Remember.Model;

namespace Remember.Web.Areas.Api.Controllers
{
    /// <summary>This class demonstrates Area support as now there are now two TaskControllers</summary>
    public class TaskController : Controller
    {
        readonly IRepository<Task> _tasks;

        public TaskController(IRepository<Task> tasks)
        {
            if (tasks == null)
                throw new ArgumentNullException("tasks");

            _tasks = tasks;
        }
        
        //
        // GET: /Api/Task/
        public ActionResult Index()
        {
            var outstandingTasks = _tasks.FindBySpecification(new IncompleteTaskSpecification());
            return Json(outstandingTasks, JsonRequestBehavior.AllowGet);
        }

    }
}
