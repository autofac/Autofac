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

using Autofac.Core;
using Autofac.Util;

namespace Autofac
{
    /// <summary>
    /// A parameter identified by name. When applied to a reflection-based
    /// component, <see cref="NamedParameter.Name"/> will be matched against
    /// the name of the component's constructor arguments. When applied to
    /// a delegate-based component, the parameter can be accessed using
    /// <see cref="ParameterExtensions.Named"/>.
    /// </summary>
    /// <example>
    /// Component with parameter:
    /// <code>
    /// public class MyComponent
    /// {
    ///     public MyComponent(int amount) { ... }
    /// }
    /// </code>
    /// Providing the parameter:
    /// <code>
    /// var builder = new ContainerBuilder();
    /// builder.RegisterType&lt;MyComponent&gt;();
    /// var container = builder.Build();
    /// var myComponent = container.Resolve&lt;MyComponent&gt;(new NamedParameter("amount", 123));
    /// </code>
    /// </example>
    public class NamedParameter : ConstantParameter
    {
        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Create a <see cref="NamedParameter"/> with the specified constant value.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The parameter value.</param>
        public NamedParameter(string name, object value)
            : base(value, pi => pi.Name == name)
        {
            Name = Enforce.ArgumentNotNullOrEmpty(name, "name");
        }
    }
}
