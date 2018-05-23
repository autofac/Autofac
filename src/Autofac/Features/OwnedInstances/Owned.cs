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
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Autofac.Core;
using Autofac.Util;

namespace Autofac.Features.OwnedInstances
{
    /// <summary>
    /// Represents a dependency that can be released by the dependent component.
    /// </summary>
    /// <typeparam name="T">The service provided by the dependency.</typeparam>
    /// <remarks>
    /// <para>
    /// Autofac automatically provides instances of <see cref="Owned{T}"/> whenever the
    /// service <typeparamref name="T"/> is registered.
    /// </para>
    /// <para>
    /// It is not necessary for <typeparamref name="T"/>, or the underlying component, to implement <see cref="System.IDisposable"/>.
    /// Disposing of the <see cref="Owned{T}"/> object is the correct way to handle cleanup of the dependency,
    /// as this will dispose of any other components created indirectly as well.
    /// </para>
    /// <para>
    /// When <see cref="Owned{T}"/> is resolved, a new <see cref="ILifetimeScope"/> is created for the
    /// underlying <typeparamref name="T"/>, and tagged with the service matching <typeparamref name="T"/>,
    /// generally a <see cref="TypedService"/>. This means that shared instances can be tied to this
    /// scope by registering them as InstancePerMatchingLifetimeScope(new TypedService(typeof(T))).
    /// </para>
    /// </remarks>
    /// <example>
    /// The component D below is disposable and implements IService:
    /// <code>
    /// public class D : IService, IDisposable
    /// {
    ///   // ...
    /// }
    /// </code>
    /// The dependent component C can dispose of the D instance whenever required by taking a dependency on
    /// <see cref="Owned{IService}"/>:
    /// <code>
    /// public class C
    /// {
    ///   IService _service;
    ///
    ///   public C(Owned&lt;IService&gt; service)
    ///   {
    ///     _service = service;
    ///   }
    ///
    ///   void DoWork()
    ///   {
    ///     _service.Value.DoSomething();
    ///   }
    ///
    ///   void OnFinished()
    ///   {
    ///     _service.Dispose();
    ///   }
    /// }
    /// </code>
    /// In general, rather than depending on <see cref="Owned{T}"/> directly, components will depend on
    /// System.Func&lt;Owned&lt;T&gt;&gt; in order to create and dispose of other components as required.
    /// </example>
    [SuppressMessage("Microsoft.ApiDesignGuidelines", "CA2213", Justification = "False positive - the lifetime does get disposed.")]
    public class Owned<T> : Disposable
    {
        private IDisposable _lifetime;

        /// <summary>
        /// Initializes a new instance of the <see cref="Owned{T}"/> class.
        /// </summary>
        /// <param name="value">The value representing the instance.</param>
        /// <param name="lifetime">An IDisposable interface through which ownership can be released.</param>
        public Owned(T value, IDisposable lifetime)
        {
            if (lifetime == null) throw new ArgumentNullException(nameof(lifetime));

            Value = value;
            _lifetime = lifetime;
        }

        /// <summary>
        /// Gets or sets the owned value.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                var lt = Interlocked.Exchange(ref _lifetime, null);
                if (lt != null)
                {
                    Value = default(T);
                    lt.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}
