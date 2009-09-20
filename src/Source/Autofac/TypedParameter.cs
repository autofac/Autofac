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

using System;
using Autofac.Util;
using Autofac.Core;

namespace Autofac
{
	/// <summary>
	/// A parameter that can supply values to sites that exactly
    /// match a specified type. When applied to a reflection-based
    /// component, <see cref="TypedParameter.Type"/> will be matched against
    /// the types of the component's constructor arguments. When applied to
    /// a delegate-based component, the parameter can be accessed using
    /// <see cref="ParameterExtensions.TypedAs"/>.
    /// </summary>
    /// <example>
    /// public class MyComponent
    /// {
    ///     public MyComponent(int amount) { ... }
    /// }
    /// 
    /// var builder = new ContainerBuilder();
    /// builder.RegisterType&lt;MyComponent&gt;();
    /// var container = builder.Build();
    /// var myComponent = container.Resolve&lt;MyComponent&gt;(
    ///     new Parameter[] { new TypedParameter(typeof(int), 123) });
    /// </example>
    public class TypedParameter : ConstantParameter
    {
        /// <summary>
        /// The type against which targets are matched.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Create a typed parameter with the specified constant value.
        /// </summary>
        /// <param name="type">The exact type to match.</param>
        /// <param name="value">The parameter value.</param>
        public TypedParameter(Type type, object value)
            : base(value, pi => pi.ParameterType == type)
        {
            Type = Enforce.ArgumentNotNull(type, "type");
        }


		/// <summary>
		/// Shortcut for creating <see cref="TypedParameter"/> 
		/// by using the <typeparamref name="T"/>
		/// </summary>
		/// <typeparam name="T">type to be used for the parameter</typeparam>
		/// <param name="value">The parameter value.</param>
		/// <returns>new typed parameter</returns>
		public static TypedParameter From<T>(T value)
		{
			return new TypedParameter(typeof(T), value);
		}
    }
}
