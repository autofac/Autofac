// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Reflection;
using Autofac.Core;

namespace Autofac
{
    /// <summary>
    /// A parameter that is identified according to an integer representing its
    /// position in an argument list. When applied to a reflection-based
    /// component, <see cref="Position"/> will be matched against
    /// the indices of the component's constructor arguments. When applied to
    /// a delegate-based component, the parameter can be accessed using
    /// <see cref="ParameterExtensions.Positional{T}"/>.
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
    /// var myComponent = container.Resolve&lt;MyComponent&gt;(new PositionalParameter(0, 123));
    /// </code>
    /// </example>
    public class PositionalParameter : ConstantParameter
    {
        /// <summary>
        /// Gets the zero-based position of the parameter.
        /// </summary>
        public int Position { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PositionalParameter"/> class.
        /// </summary>
        /// <param name="position">The zero-based position of the parameter.</param>
        /// <param name="value">The parameter value.</param>
        public PositionalParameter(int position, object value)
            : base(value, pi => pi.Position == position && (pi.Member is ConstructorInfo))
        {
            if (position < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(position));
            }

            Position = position;
        }
    }
}
