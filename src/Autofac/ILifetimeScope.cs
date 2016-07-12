// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Core.Resolving;

namespace Autofac
{
    /// <summary>
    /// An <see cref="ILifetimeScope"/> tracks the instantiation of component instances.
    /// It defines a boundary in which instances are shared and configured.
    /// Disposing an <see cref="ILifetimeScope"/> will dispose the components that were
    /// resolved through it.
    /// </summary>
    /// <example>
    /// <code>
    /// // See IContainer for definition of the container variable
    /// using (var requestScope = container.BeginLifetimeScope())
    /// {
    ///     // Note that handler is resolved from requestScope, not
    ///     // from the container:
    ///
    ///     var handler = requestScope.Resolve&lt;IRequestHandler&gt;();
    ///     handler.Handle(request);
    ///
    ///     // When requestScope is disposed, all resources used in processing
    ///     // the request will be released.
    /// }
    /// </code>
    /// </example>
    /// <remarks>
    /// All long-running applications should resolve components via an
    /// <see cref="ILifetimeScope"/>. Choosing the duration of the lifetime is application-
    /// specific. The standard Autofac WCF and ASP.NET/MVC integrations are already configured
    /// to create and release <see cref="ILifetimeScope"/>s as appropriate. For example, the
    /// ASP.NET integration will create and release an <see cref="ILifetimeScope"/> per HTTP
    /// request.
    /// Most <see cref="ILifetimeScope"/> functionality is provided by extension methods
    /// on the inherited <see cref="IComponentContext"/> interface.
    /// </remarks>
    /// <seealso cref="IContainer"/>
    /// <seealso cref="IComponentContext"/>
    /// <seealso cref="IRegistrationBuilder{TLimit,TActivatorData,TRegistrationStyle}.InstancePerMatchingLifetimeScope"/>
    /// <seealso cref="IRegistrationBuilder{TLimit,TActivatorData,TRegistrationStyle}.InstancePerLifetimeScope"/>
    /// <seealso cref="InstanceSharing"/>
    /// <seealso cref="IComponentLifetime"/>
    public interface ILifetimeScope : IComponentContext, IDisposable
    {
        /// <summary>
        /// Begin a new nested scope. Component instances created via the new scope
        /// will be disposed along with it.
        /// </summary>
        /// <returns>A new lifetime scope.</returns>
        ILifetimeScope BeginLifetimeScope();

        /// <summary>
        /// Begin a new nested scope. Component instances created via the new scope
        /// will be disposed along with it.
        /// </summary>
        /// <param name="tag">The tag applied to the <see cref="ILifetimeScope"/>.</param>
        /// <returns>A new lifetime scope.</returns>
        ILifetimeScope BeginLifetimeScope(object tag);

        /// <summary>
        /// Begin a new nested scope, with additional components available to it.
        /// Component instances created via the new scope
        /// will be disposed along with it.
        /// </summary>
        /// <remarks>
        /// The components registered in the sub-scope will be treated as though they were
        /// registered in the root scope, i.e., SingleInstance() components will live as long
        /// as the root scope.
        /// </remarks>
        /// <param name="configurationAction">Action on a <see cref="ContainerBuilder"/>
        /// that adds component registrations visible only in the new scope.</param>
        /// <returns>A new lifetime scope.</returns>
        ILifetimeScope BeginLifetimeScope(Action<ContainerBuilder> configurationAction);

        /// <summary>
        /// Begin a new nested scope, with additional components available to it.
        /// Component instances created via the new scope
        /// will be disposed along with it.
        /// </summary>
        /// <remarks>
        /// The components registered in the sub-scope will be treated as though they were
        /// registered in the root scope, i.e., SingleInstance() components will live as long
        /// as the root scope.
        /// </remarks>
        /// <param name="tag">The tag applied to the <see cref="ILifetimeScope"/>.</param>
        /// <param name="configurationAction">Action on a <see cref="ContainerBuilder"/>
        /// that adds component registrations visible only in the new scope.</param>
        /// <returns>A new lifetime scope.</returns>
        ILifetimeScope BeginLifetimeScope(object tag, Action<ContainerBuilder> configurationAction);

        /// <summary>
        /// Gets the disposer associated with this <see cref="ILifetimeScope"/>.
        /// Component instances can be associated with it manually if required.
        /// </summary>
        /// <remarks>Typical usage does not require interaction with this member- it
        /// is used when extending the container.</remarks>
        IDisposer Disposer { get; }

        /// <summary>
        /// Gets the tag applied to the <see cref="ILifetimeScope"/>.
        /// </summary>
        /// <remarks>Tags allow a level in the lifetime hierarchy to be identified.
        /// In most applications, tags are not necessary.</remarks>
        /// <seealso cref="IRegistrationBuilder{TLimit,TActivatorData,TRegistrationStyle}.InstancePerMatchingLifetimeScope"/>
        object Tag { get; }

        /// <summary>
        /// Fired when a new scope based on the current scope is beginning.
        /// </summary>
        event EventHandler<LifetimeScopeBeginningEventArgs> ChildLifetimeScopeBeginning;

        /// <summary>
        /// Fired when this scope is ending.
        /// </summary>
        event EventHandler<LifetimeScopeEndingEventArgs> CurrentScopeEnding;

        /// <summary>
        /// Fired when a resolve operation is beginning in this scope.
        /// </summary>
        event EventHandler<ResolveOperationBeginningEventArgs> ResolveOperationBeginning;
    }
}
