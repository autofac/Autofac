// This software is part of the Autofac IoC container
// Copyright © 2012 Autofac Contributors
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

namespace Autofac.Plugin
{
    /// <summary>
    /// Entry point for resolving platform specific plugins.
    /// </summary>
    internal static class PlatformPlugin
    {
        /// <summary>
        /// Name for the full .NET 4.0 platform.
        /// </summary>
        internal static readonly string FullPlatformName = "Full";

        static readonly string[] KownPlatformNames = new[] {FullPlatformName};
        private static IPluginResolver _resolver = new ProbingPluginResolver(KownPlatformNames);

        /// <summary>
        /// Resolves the specified plugin.
        /// </summary>
        /// <typeparam name="T">The type of the plugin to resolve.</typeparam>
        /// <returns>The plugin instance.</returns>
        /// <exception cref="System.PlatformNotSupportedException">
        /// Thrown when the plugin is not supported by the current platform.
        /// </exception>
        public static T Resolve<T>() where T : class
        {
            var value = ResolveOptional<T>();

            if (value == null)
                throw new PlatformNotSupportedException(
                    string.Format(PlatformPluginResources.PluginNotSupported, typeof(T).FullName));

            return value;
        }

        /// <summary>
        /// Resolves the specified plugin.
        /// </summary>
        /// <typeparam name="T">The type of the plugin to resolve.</typeparam>
        /// <returns>The plugin instance if resolved; otherwise, <c>null</c>.</returns>
        public static T ResolveOptional<T>() where T : class
        {
            return (T)_resolver.ResolveOptional(typeof(T));
        }

        /// <summary>
        /// Sets the resolver.
        /// </summary>
        /// <param name="resolver">The resolver.</param>
        /// <remarks>For unit test usage only.</remarks>
        internal static void SetResolver(IPluginResolver resolver)
        {
            _resolver = resolver;
        }

        /// <summary>
        /// Resets the resolver.
        /// </summary>
        /// <remarks>For unit test usage only.</remarks>
        internal static void ResetResolver()
        {
            _resolver = new ProbingPluginResolver(KownPlatformNames);
        }
    }
}
