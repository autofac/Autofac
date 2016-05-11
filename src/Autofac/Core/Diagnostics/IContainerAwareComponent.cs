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

namespace Autofac.Core.Diagnostics
{
    /// <summary>
    /// Marks a module as container-aware (for the purposes of attaching to diagnostic events.)
    /// </summary>
    [Obsolete("Use the more general Autofac.IStartable interface instead. The IContainer parameter can be emulated when implementing IStartable by taking a dependency on IComponentContext or ILifetimeScope.", true)]
    public interface IContainerAwareComponent
    {
        /// <summary>
        /// Initialise the module with the container into which it is being registered.
        /// </summary>
        /// <param name="container">The container.</param>
        void SetContainer(IContainer container);
    }
}
