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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Autofac.Util
{
    /// <summary>
    /// Extension methods for <see cref="System.Reflection.Assembly"/>.
    /// </summary>
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Safely returns the set of loadable types from an assembly.
        /// </summary>
        /// <param name="assembly">The <see cref="System.Reflection.Assembly"/> from which to load types.</param>
        /// <returns>
        /// The set of types from the <paramref name="assembly" />, or the subset
        /// of types that could be loaded if there was any error.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="assembly" /> is <see langword="null" />.
        /// </exception>
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            // Algorithm from StackOverflow answer here:
            // http://stackoverflow.com/questions/7889228/how-to-prevent-reflectiontypeloadexception-when-calling-assembly-gettypes
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            try
            {
                return assembly.DefinedTypes.Select(t => t.AsType());
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null);
            }
        }
    }
}
