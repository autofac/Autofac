using System;
using System.Collections.Generic;
using Remember.Model;

namespace Remember.Web.PresentationModel
{
    public class TaskList
    {
        private IEnumerable<Task> _tasks;

        public TaskList(IEnumerable<Task> tasks)
        {
            if (tasks == null)
                throw new ArgumentNullException("tasks");

            _tasks = tasks;
        }

        public IEnumerable<Task> Tasks
        {
            get
            {
                return _tasks;
            }
        }
    }
}
