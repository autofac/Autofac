using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.DomainServices.Client;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Remember.Model;
using Remember.Web;

namespace DomainServicesExample
{
    public partial class MainPage : UserControl
    {
        private TaskDomainContext _taskContext = new TaskDomainContext();

        public MainPage()
        {
            InitializeComponent();
            var op = this._taskContext.GetTasks(this.LoadResultsIntoGrid, null);
        }

        private void LoadResultsIntoGrid(InvokeOperation<IEnumerable<Task>> op)
        {
            this.TaskGrid.ItemsSource = op.Value;
        }
    }
}
