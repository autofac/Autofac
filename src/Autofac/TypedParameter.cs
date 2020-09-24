// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Autofac.Core;

namespace Autofac
{
    /// <summary>
    /// A parameter that can supply values to sites that exactly
    /// match a specified type. When applied to a reflection-based
    /// component, <see cref="Type"/> will be matched against
    /// the types of the component's constructor arguments. When applied to
    /// a delegate-based component, the parameter can be accessed using
    /// <see cref="ParameterExtensions.TypedAs{T}"/>.
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
    /// var myComponent = container.Resolve&lt;MyComponent&gt;(new TypedParameter(typeof(int), 123));
    /// </code>
    /// </example>
    public class TypedParameter : ConstantParameter
    {
        /// <summary>
        /// Gets the type against which targets are matched.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public Type Type { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypedParameter"/> class.
        /// </summary>
        /// <param name="type">The exact type to match.</param>
        /// <param name="value">The parameter value.</param>
        public TypedParameter(Type type, object? value)
            : base(value, pi => pi.ParameterType == type)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        /// <summary>
        /// Shortcut for creating <see cref="TypedParameter"/>
        /// by using the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type to be used for the parameter.</typeparam>
        /// <param name="value">The parameter value.</param>
        /// <returns>New typed parameter.</returns>
        public static TypedParameter From<T>(T value)
        {
            return new TypedParameter(typeof(T), value);
        }
    }
}
