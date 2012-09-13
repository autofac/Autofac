using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Windows.UI.Xaml;

namespace AutofacAppCertTest
{
    /// <summary>
    /// Main page for the AppCert test application.
    /// </summary>
    public sealed partial class MainPage : AutofacAppCertTest.Common.LayoutAwarePage
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
            if (pageState != null && pageState.ContainsKey("dateOutputText"))
            {
                // If the app was previously suspended/terminated, put the text back.
                this.dateOutput.Text = pageState["dateOutputText"].ToString();
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
            pageState["dateOutputText"] = this.dateOutput.Text;
        }

        /// <summary>
        /// Handles the "Get Date" button click event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        /// <remarks>
        /// <para>
        /// This method is where we resolve a service from Autofac and make use of it.
        /// Very simple mechanism just to get Autofac integrated with Metro, nothing
        /// fancy.
        /// </para>
        /// </remarks>
        private void GetDate_Click(object sender, RoutedEventArgs e)
        {
            var provider = App.ApplicationContainer.Resolve<IDateProvider>();
            this.dateOutput.Text = provider.Now.ToString();
        }
    }
}
