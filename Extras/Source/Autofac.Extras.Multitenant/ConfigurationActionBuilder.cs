using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security;
using Autofac;

namespace AutofacContrib.Multitenant
{
    /// <summary>
    /// Allows you to build up a set of configuration actions that can be used
    /// all at once to configure a new <see cref="Autofac.ILifetimeScope"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Usually when you are building an <see cref="Autofac.ILifetimeScope"/>
    /// and you wish to update the registrations in the new scope, you are
    /// required to pass in a single lambda configuration action. Sometimes,
    /// though, there is a need to perform some logic, or otherwise programmatically
    /// register several things into the new lifetime across different calls or
    /// in different locations in your application.
    /// </para>
    /// <para>
    /// This builder allows you to collect a set of configuration actions and
    /// use a final <see cref="AutofacContrib.Multitenant.ConfigurationActionBuilder.Build"/>
    /// method to create a single aggregate action that can be used to finally
    /// create the lifetime scope.
    /// </para>
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "This builder behaves like a collection but it is specifically for building an aggregate action, not just collecting them.")]
    public class ConfigurationActionBuilder : List<Action<ContainerBuilder>>
    {
        /// <summary>
        /// Creates an aggregated action based on the list of actions contained
        /// in the builder.
        /// </summary>
        /// <returns>
        /// A <see cref="System.Action{T}"/> that contains the aggregate set of
        /// registered actions that have been added to this builder.
        /// </returns>
        public Action<ContainerBuilder> Build()
        {
            var list = this.AsReadOnly();
            if (list.Count == 0)
            {
                return (ContainerBuilder b) => { };
            }
            else
            {
                return (Action<ContainerBuilder>)Action<ContainerBuilder>.Combine(list.ToArray());
            }
        }
    }
}
