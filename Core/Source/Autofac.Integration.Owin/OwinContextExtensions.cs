using System;
using System.Security;
using Microsoft.Owin;

namespace Autofac.Integration.Owin
{
    /// <summary>
    /// Extension methods for using Autofac within an OWIN context.
    /// </summary>
    public static class OwinContextExtensions
    {
        /// <summary>
        /// Gets the current Autofac lifetime scope from the OWIN context.
        /// </summary>
        /// <param name="context">The OWIN context.</param>
        /// <returns>The current lifetime scope.</returns>
        [SecuritySafeCritical]
        public static ILifetimeScope GetAutofacLifetimeScope(this IOwinContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            return context.Get<ILifetimeScope>(Constants.OwinLifetimeScopeKey);
        }
    }
}
