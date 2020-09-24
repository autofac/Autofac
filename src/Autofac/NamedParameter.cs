// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;
using Autofac.Util;

namespace Autofac
{
    /// <summary>
    /// A parameter identified by name. When applied to a reflection-based
    /// component, <see cref="Name"/> will be matched against
    /// the name of the component's constructor arguments. When applied to
    /// a delegate-based component, the parameter can be accessed using
    /// <see cref="ParameterExtensions.Named{T}"/>.
    /// </summary>
    /// <example>
    /// <para>
    /// Component with parameter...
    /// </para>
    /// <code>
    /// public class MyComponent
    /// {
    ///     public MyComponent(int amount) { ... }
    /// }
    /// </code>
    /// <para>
    /// Providing the parameter...
    /// </para>
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
        /// Gets the name of the parameter.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedParameter"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The parameter value.</param>
        public NamedParameter(string name, object value)
            : base(value, pi => pi.Name == name) =>
                Name = Enforce.ArgumentNotNullOrEmpty(name, "name");
    }
}
