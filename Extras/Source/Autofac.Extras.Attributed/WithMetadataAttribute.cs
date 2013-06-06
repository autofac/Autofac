// This software is part of the Autofac IoC container
// Copyright © 2013 Autofac Contributors
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
using System.Linq;

namespace Autofac.Extras.Attributed
{
    /// <summary>
    /// Provides an annotation to filter constructor dependencies 
    /// according to their specified metadata.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This attribute allows constructor dependencies to be filtered by metadata.
    /// By marking your dependencies with this attribute and associating
    /// a metadata filter with your type registration, you can be selective
    /// about which service registration should be used to provide the
    /// dependency.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>
    /// A simple example might be registration of a specific logger type to be
    /// used by a class. If many loggers are registered with the <c>LoggerName</c>
    /// metadata, the consumer can simply specify the filter as an attribute to
    /// the constructor parameter.
    /// </para>
    /// <code lang="C#">
    /// public class Manager
    /// {
    ///   public Manager([WithMetadata("LoggerName", "Manager")] ILogger logger)
    ///   {
    ///     // ...
    ///   }
    /// }
    /// </code>
    /// <para>
    /// The same thing can be done for enumerable:
    /// </para>
    /// <code lang="C#">
    /// public class SolutionExplorer
    /// {
    ///   public SolutionExplorer(
    ///     [WithMetadata("Target", "Solution")] IEnumerable&lt;IAdapter&gt; adapters,
    ///     [WithMetadata("LoggerName", "Solution")] ILogger logger)
    ///   {
    ///     this.Adapters = adapters.ToList();
    ///     this.Logger = logger;
    ///   }
    /// }
    /// </code>
    /// <para>
    /// When registering your components, the associated metadata on the
    /// dependencies will be used. Be sure to specify the
    /// <see cref="Autofac.Extras.Attributed.AutofacAttributeExtensions.WithMetadataFilter{TLimit, TReflectionActivatorData, TStyle}" />
    /// extension on the type with the filtered constructor parameters.
    /// </para>
    /// <code lang="C#">
    /// var builder = new ContainerBuilder();
    /// builder.RegisterModule(new AttributedMetadataModule());
    /// 
    /// // Attach metadata to the components getting filtered
    /// builder.RegisterType&lt;ConsoleLogger&gt;().WithMetadata(&quot;LoggerName&quot;, &quot;Solution&quot;).As&lt;ILogger&gt;();
    /// builder.RegisterType&lt;FileLogger&gt;().WithMetadata(&quot;LoggerName&quot;, &quot;Other&quot;).As&lt;ILogger&gt;();
    /// 
    /// // Attach the filtering behavior to the component with the constructor
    /// builder.RegisterType&lt;SolutionExplorer&gt;().WithMetadataFilter();
    /// 
    /// var container = builder.Build();
    /// 
    /// // The resolved instance will have the appropriate services in place
    /// var explorer = container.Resolve&lt;SolutionExplorer&gt;();
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class WithMetadataAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WithMetadataAttribute"/> class, 
        /// specifying the <paramref name="key"/> and <paramref name="value"/> that the 
        /// dependency should have in order to satisfy the parameter.
        /// </summary>
        public WithMetadataAttribute(string key, object value)
        {
            this.Key = key;
            this.Value = value;
        }

        /// <summary>
        /// Gets the key the dependency is expected to have to satisfy the parameter.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Gets the value the dependency is expected to have to satisfy the parameter.
        /// </summary>
        public object Value { get; private set; }
    }

}