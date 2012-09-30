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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.IO;

namespace Autofac.Plugin
{
    /// <summary>
    /// A plugin resolver that probes for platform-specific plugins by dynamically
    /// looking for concrete types in platform-specific assemblies, such as Autofac.Full.
    /// </summary>
    internal class ProbingPluginResolver : IPluginResolver
    {
        readonly string[] _platformNames;
        readonly Func<string, Assembly> _assemblyLoader;
        readonly object _lock = new object();
        readonly Dictionary<Type, object> _adapters = new Dictionary<Type, object>();
        readonly string _baseAssemblyName = new AssemblyName(Assembly.GetExecutingAssembly().FullName).Name;
        Assembly _assembly;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProbingPluginResolver" /> class.
        /// </summary>
        /// <param name="platformNames">The platform names.</param>
        public ProbingPluginResolver(params string[] platformNames) : this(Assembly.Load, platformNames)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProbingPluginResolver" /> class.
        /// </summary>
        /// <param name="assemblyLoader">The assembly loader.</param>
        /// <param name="platformNames">The platform names.</param>
        public ProbingPluginResolver(Func<string, Assembly> assemblyLoader, params string[] platformNames)
        {
            Debug.Assert(platformNames != null);
            Debug.Assert(assemblyLoader != null);

            _platformNames = platformNames;
            _assemblyLoader = assemblyLoader;
        }

        /// <summary>
        /// Resolves the specified plugin.
        /// </summary>
        /// <param name="type">The type of the plugin.</param>
        /// <returns>The plugin instance.</returns>
        public object Resolve(Type type)
        {
            return ResolvePlugin(type, true);
        }

        /// <summary>
        /// Resolves the specified plugin.
        /// </summary>
        /// <param name="type">The type of the plugin.</param>
        /// <returns>The plugin instance if resolved; otherwise, <c>null</c>.</returns>
        public object ResolveOptional(Type type)
        {
            return ResolvePlugin(type, false);
        }

        object ResolvePlugin(Type type, bool throwIfNotFound)
        {
            Debug.Assert(type != null);

            lock (_lock)
            {
                object instance;
                if (!_adapters.TryGetValue(type, out instance))
                {
                    var assembly = GetPlatformSpecificAssembly(type, throwIfNotFound);
                    if (assembly == null) return null;

                    instance = ResolvePlugin(assembly, type, throwIfNotFound);
                    _adapters.Add(type, instance);
                }

                return instance;
            }
        }

        Assembly GetPlatformSpecificAssembly(Type type, bool throwIfNotFound)
        {
            if (_assembly == null)
            {
                _assembly = ProbeForPlatformSpecificAssembly();
                if (_assembly == null && throwIfNotFound)
                    throw new InvalidOperationException(
                        string.Format(ProbingPluginResolverResources.AssemblyNotSupported, type.FullName));
            }

            return _assembly;
        }

        Assembly ProbeForPlatformSpecificAssembly()
        {
            return _platformNames.Select(ProbeForPlatformSpecificAssembly)
                .FirstOrDefault(assembly => assembly != null);
        }

        Assembly ProbeForPlatformSpecificAssembly(string platformName)
        {
            var simpleName = _baseAssemblyName + "." + platformName;
            var assemblyName = new AssemblyName(GetType().Assembly.FullName) {Name = simpleName};

            try
            {
                return _assemblyLoader(assemblyName.FullName);
            }
            catch (FileNotFoundException)
            {
            }

            return null;
        }

        static object ResolvePlugin(Assembly assembly, Type interfaceType, bool throwIfNotFound)
        {
            var typeName = BuildPluginTypeName(interfaceType);
            var type = assembly.GetType(typeName, false);

            if (type != null) return Activator.CreateInstance(type);

            if (throwIfNotFound)
                throw new InvalidOperationException(
                    string.Format(ProbingPluginResolverResources.TypeNotFound, interfaceType.FullName));

            return null;
        }

        static string BuildPluginTypeName(Type interfaceType)
        {
            Debug.Assert(interfaceType.IsInterface);
            Debug.Assert(interfaceType.DeclaringType == null);
            Debug.Assert(interfaceType.Name.StartsWith("I", StringComparison.OrdinalIgnoreCase));

            // For example, if we're looking for an implementation of Autofac.Core.IRegistrationSourceProvider, 
            // then we'll look for Autofac.Core.IRegistrationSourceProvider.
            return interfaceType.Namespace + "." + interfaceType.Name.Substring(1);
        }
    }
}
