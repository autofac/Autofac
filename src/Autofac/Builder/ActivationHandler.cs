using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Builder
{
    /// <summary>
    /// Utilities to handle common activation scenarios
    /// </summary>
    public static class ActivationHandler
    {
        /// <summary>
        /// Inject properties from the context into the newly
        /// activated instance.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Autofac.ActivatingEventArgs"/> instance containing the event data.</param>
        public static void InjectProperties(object sender, ActivatingEventArgs e)
        {
            Enforce.ArgumentNotNull(sender, "sender");
            Enforce.ArgumentNotNull(e, "e");
            e.Context.InjectProperties(e.Instance);
        }

        /// <summary>
        /// Inject properties from the context into the newly
        /// activated instance unless they're non null on the instance.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Autofac.ActivatingEventArgs"/> instance containing the event data.</param>
        public static void InjectUnsetProperties(object sender, ActivatingEventArgs e)
        {
            Enforce.ArgumentNotNull(sender, "sender");
            Enforce.ArgumentNotNull(e, "e");
            e.Context.InjectUnsetProperties(e.Instance);
        }
    }
}
