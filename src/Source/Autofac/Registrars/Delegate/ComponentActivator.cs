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

using System.Collections.Generic;

namespace Autofac.Registrars.Delegate
{
    /// <summary>
    /// Implementations of the ComponentActivator type will
    /// create an instance of a component, using the provided container
    /// to resolve its dependencies.
    /// </summary>
    /// <param name="context">The container from which the component's
    /// dependencies may be resolved.</param>
    /// <returns>An instance of the component.</returns>
    /// <remarks>
    /// A creation delegate can be used when the method of initialisation
    /// of a component is complex, its constructors require parameters
    /// that must be obtained from outside the container.
    /// The delegate should always return a unique instance.
    /// </remarks>
    public delegate T ComponentActivator<T>(IComponentContext context);

    /// <summary>
    /// Implementations of the ComponentActivator type will
    /// create an instance of a component, using the provided container
    /// to resolve its dependencies.
    /// </summary>
    /// <param name="context">The container from which the component's
    /// dependencies may be resolved.</param>
    /// <param name="parameters">The activation parameters.</param>
    /// <returns>An instance of the component.</returns>
    /// <remarks>
    /// A creation delegate can be used when the method of initialisation
    /// of a component is complex, its constructors require parameters
    /// that must be obtained from outside the container.
    /// The delegate should always return a unique instance.
    /// </remarks>
    public delegate T ComponentActivatorWithParameters<T>(IComponentContext context, IEnumerable<Parameter> parameters);
}
