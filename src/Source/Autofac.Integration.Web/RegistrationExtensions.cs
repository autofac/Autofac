using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Registrars;

namespace Autofac.Integration.Web
{
    /// <summary>
    /// Extends registrars to support an HttpContextScoped() member.
    /// </summary>
    public static class RegistrationExtensions
    {
        /// <summary>
        /// Share one instance of the component within the context of a single
        /// HTTP request (synonym for <see cref="IRegistrar{T}.ContainerScoped()"/>).
        /// </summary>
        /// <typeparam name="TSyntax">Builder syntax type.</typeparam>
        /// <param name="registrar">The registrar being used to create the registration.</param>
        /// <returns>The registrar, allowing registration to continue.</returns>
        public static TSyntax HttpRequestScoped<TSyntax>(this TSyntax registrar)
            where TSyntax : class, IRegistrar<TSyntax>
        {
            if (registrar == null) throw new ArgumentNullException("registrar");

            return registrar.ContainerScoped();
        }
    }
}
