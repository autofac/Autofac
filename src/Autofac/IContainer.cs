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

namespace Autofac
{
    /// <summary>
    /// Creates, wires dependencies and manages lifetime for a set of components.
    /// Most instances of <see cref="IContainer"/> are created
    /// by a <see cref="ContainerBuilder"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// // See ContainerBuilder for the definition of the builder variable
    /// using (var container = builder.Build())
    /// {
    ///     var program = container.Resolve&lt;Program&gt;();
    ///     program.Run();
    /// }
    /// </code>
    /// </example>
    /// <remarks>
    /// Most <see cref="IContainer"/> functionality is provided by extension methods
    /// on the inherited <see cref="IComponentContext"/> interface.
    /// </remarks>
    /// <seealso cref="ILifetimeScope"/>
    /// <seealso cref="IComponentContext"/>
    /// <seealso cref="ResolutionExtensions"/>
    /// <seealso cref="ContainerBuilder"/>
    public interface IContainer : ILifetimeScope
    {
    }
}
