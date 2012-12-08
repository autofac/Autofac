using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.DomainServices.Hosting;
using System.ServiceModel.DomainServices.Server;
using Remember.Model;
using Remember.Persistence;

namespace Remember.Web
{
    [EnableClientAccess]
    public class TaskDomainService : DomainService
    {
        readonly IRepository<Task> _tasks;

        public TaskDomainService(IRepository<Task> tasks)
        {
            if (tasks == null)
            {
                throw new ArgumentNullException("tasks");
            }
            _tasks = tasks;
        }

        public IEnumerable<Task> GetTasks()
        {
            return _tasks.FindBySpecification(new IncompleteTaskSpecification());
        }
    }
}


