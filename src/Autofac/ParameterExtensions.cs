// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Autofac.Util;

namespace Autofac
{
    /// <summary>
    /// Extension methods that simplify extraction of parameter values from
    /// an <see cref="IEnumerable{T}"/> where T is <see cref="Autofac.Core.Parameter"/>.
    /// Each method returns the first matching parameter value, or throws an exception if
    /// none is provided.
    /// </summary>
    /// <example>
    /// At configuration time, delegate registrations can retrieve parameter values using
    /// the methods <see cref="Named{T}"/>, <see cref="Positional{T}"/> and <see cref="TypedAs{T}"/>:
    /// <code>
    /// builder.Register((c, p) => new FtpClient(p.Named&lt;string&gt;("server")));
    /// </code>
    /// These parameters can be provided at resolution time:
    /// <code>
    /// container.Resolve&lt;FtpClient&gt;(new NamedParameter("server", "ftp.example.com"));
    /// </code>
    /// Alternatively, the parameters can be provided via a <i>Generated Factory</i> - http://code.google.com/p/autofac/wiki/DelegateFactories.
    /// </example>
    public static class ParameterExtensions
    {
        /// <summary>
        /// Retrieve a named parameter value from a <see cref="NamedParameter"/> instance.
        /// </summary>
        /// <typeparam name="T">The type to which the returned value will be cast.</typeparam>
        /// <param name="parameters">The available parameters to choose from.</param>
        /// <param name="name">The name of the parameter to select.</param>
        /// <returns>The value of the selected parameter.</returns>
        /// <seealso cref="NamedParameter"/>
        public static T Named<T>(this IEnumerable<Parameter> parameters, string name)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            Enforce.ArgumentNotNullOrEmpty(name, "name");

            return ConstantValue<NamedParameter, T>(parameters, c => c.Name == name);
        }

        /// <summary>
        /// Retrieve a positional parameter value from a <see cref="PositionalParameter"/> instance.
        /// </summary>
        /// <typeparam name="T">The type to which the returned value will be cast.</typeparam>
        /// <param name="parameters">The available parameters to choose from.</param>
        /// <param name="position">The zero-based position of the parameter to select.</param>
        /// <returns>The value of the selected parameter.</returns>
        /// <remarks>The position value is the one associated with the parameter when
        /// it was constructed, <b>not</b> its index into the <paramref name="parameters"/>
        /// sequence.</remarks>
        /// <seealso cref="PositionalParameter"/>
        public static T Positional<T>(this IEnumerable<Parameter> parameters, int position)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (position < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(position));
            }

            return ConstantValue<PositionalParameter, T>(parameters, c => c.Position == position);
        }

        /// <summary>
        /// Retrieve a typed parameter value from a <see cref="TypedParameter"/> instance.
        /// </summary>
        /// <typeparam name="T">The type to which the returned value will be cast.</typeparam>
        /// <param name="parameters">The available parameters to choose from.</param>
        /// <returns>The value of the selected parameter.</returns>
        /// <seealso cref="TypedParameter"/>
        public static T TypedAs<T>(this IEnumerable<Parameter> parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            return ConstantValue<TypedParameter, T>(parameters, c => c.Type == typeof(T));
        }

        private static TValue ConstantValue<TParameter, TValue>(IEnumerable<Parameter> parameters, Func<TParameter, bool> predicate)
            where TParameter : ConstantParameter
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return parameters
                .OfType<TParameter>()
                .Where(predicate)
                .Select(p => p.Value)
                .Cast<TValue>()
                .First();
        }
    }
}
