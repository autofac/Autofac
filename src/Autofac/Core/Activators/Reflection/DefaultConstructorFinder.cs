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
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Autofac.Core.Activators.Reflection
{
    /// <summary>
    /// Finds constructors that match a finder function.
    /// </summary>
    public class DefaultConstructorFinder : IConstructorFinder
    {
        private readonly Func<Type, ConstructorInfo[]> _finder;

        private static readonly ConcurrentDictionary<Type, ConstructorInfo[]> DefaultPublicConstructorsCache = new ConcurrentDictionary<Type, ConstructorInfo[]>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultConstructorFinder" /> class.
        /// </summary>
        /// <remarks>
        /// Default to selecting all public constructors.
        /// </remarks>
        public DefaultConstructorFinder()
          : this(GetDefaultPublicConstructors)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultConstructorFinder" /> class.
        /// </summary>
        /// <param name="finder">The finder function.</param>
        public DefaultConstructorFinder(Func<Type, ConstructorInfo[]> finder)
        {
            if (finder == null) throw new ArgumentNullException(nameof(finder));

            _finder = finder;
        }

        /// <summary>
        /// Finds suitable constructors on the target type.
        /// </summary>
        /// <param name="targetType">Type to search for constructors.</param>
        /// <returns>Suitable constructors.</returns>
        public ConstructorInfo[] FindConstructors(Type targetType)
        {
            return _finder(targetType);
        }

        private static ConstructorInfo[] GetDefaultPublicConstructors(Type type)
        {
            var retval = DefaultPublicConstructorsCache.GetOrAdd(
                type, t => t.GetTypeInfo().DeclaredConstructors.Where(c => c.IsPublic).ToArray());

            if (retval.Length == 0)
            {
                throw new NoConstructorsFoundException(type);
            }

            return retval;
        }
    }
}
