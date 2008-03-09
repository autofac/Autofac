using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using Remember.Model;
using System.Collections.Generic;

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
