using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Autofac.Tests.AppCert.Testing;
using Windows.UI.Xaml;

namespace Autofac.Tests.AppCert
{
    /// <summary>
    /// Main page for the AppCert test application.
    /// </summary>
    public sealed partial class MainPage : Autofac.Tests.AppCert.Common.LayoutAwarePage
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MainPage"/>.
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (pageState != null && pageState.ContainsKey("testResultContent"))
            {
                // If the app was previously suspended/terminated, put the text back.
                this.testResults.Text = pageState["testResultContent"].ToString();
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            // Save the date output text when the app is suspended/terminated.
            pageState["testResultContent"] = this.testResults.Text;
        }

        /// <summary>
        /// Handles the "Run Tests" button click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        /// <remarks>
        /// <para>
        /// This method is where we execute a few tests that will get
        /// Autofac working in a Metro environment. Nothing too fancy.
        /// </para>
        /// </remarks>
        private void RunTests_Click(object sender, RoutedEventArgs e)
        {
            var runner = new TestRunner("Autofac.Tests.AppCert.Tests");
            var sb = new StringBuilder();
            var results = runner.ExecuteTests().ToList();
            sb.AppendFormat("{0} tests | {1} success | {2} fail", results.Count(), results.Count(r => r.Success), results.Count(r => !r.Success));
            sb.AppendLine();
            foreach (var result in results)
            {
                if (result.Success)
                {
                    sb.AppendFormat("☺ {0}", result.TestMethod.Name);
                    sb.AppendLine();
                }
                else
                {
                    sb.AppendFormat("*** FAIL: {0}", result.TestMethod.Name);
                    sb.AppendLine();
                    if (!String.IsNullOrEmpty(result.Message))
                    {
                        sb.AppendLine(result.Message);
                    }
                }
            }
            this.testResults.Text = sb.ToString();
        }
    }
}
