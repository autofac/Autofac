// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2008 Autofac Contributors
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

namespace Autofac.Integration.Mef
{
    /// <summary>
    /// Support the <see cref="System.Lazy{T}"/> and <see cref="System.Lazy{T, TMetadata}"/>
    /// types automatically whenever type T is registered with the container.
    /// When a dependency of a lazy type is used, the instantiation of the underlying
    /// component will be delayed until the Value property is first accessed.
    /// </summary>
    /// <example>
    /// To enable the module, register it with the container.
    /// <code>
    /// var builder = new ContainerBuilder();
    /// builder.RegisterModule(new LazyDependencyModule());
    /// // Register other components and modules...
    /// var container = builder.Build();
    /// </code>
    /// The container will now resolve references of any lazy type:
    /// <code>
    /// var lazyFoo = container.Resolve&lt;Lazy&lt;IFoo&gt;&gt;();
    /// </code>
    /// </example>
    public class LazyDependencyModule : Module
    {
        /// <summary>
        /// Registers functionaity from the module with the container.
        /// </summary>
        /// <param name="moduleBuilder">Module builder with which to register.</param>
        protected override void Load(ContainerBuilder moduleBuilder)
        {
            base.Load(moduleBuilder);

            moduleBuilder.RegisterSource(new LazyRegistrationSource());
            moduleBuilder.RegisterSource(new LazyWithMetadataRegistrationSource());
        }
    }
}
