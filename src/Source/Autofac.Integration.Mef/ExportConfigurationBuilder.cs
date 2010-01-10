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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace Autofac.Integration.Mef
{
    /// <summary>
    /// Configures an Export on an Autofac component.
    /// </summary>
    public class ExportConfigurationBuilder
    {
        string _contractName;
        readonly IDictionary<string, object> _metadata = new Dictionary<string, object>();

        internal string ContractName { get { return _contractName; } }
        internal IDictionary<string, object> Metadata { get { return _metadata; } }

        /// <summary>
        /// Export the component under typed contract <typeparamref name="TContract"/>.
        /// </summary>
        /// <typeparam name="TContract">Contract type.</typeparam>
        /// <returns>Builder for additional configuration.</returns>
        public ExportConfigurationBuilder As<TContract>()
        {
            WithMetadata(CompositionConstants.ExportTypeIdentityMetadataName, AttributedModelServices.GetTypeIdentity(typeof(TContract)));
            _contractName = AttributedModelServices.GetContractName(typeof(TContract));
            return this;
        }

        /// <summary>
        /// Export the component under named contract <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="TExportedValue">Exported value type.</typeparam>
        /// <param name="name">Contract name.</param>
        /// <returns>Builder for additional configuration.</returns>
        public ExportConfigurationBuilder AsNamed<TExportedValue>(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            WithMetadata(CompositionConstants.ExportTypeIdentityMetadataName, AttributedModelServices.GetTypeIdentity(typeof(TExportedValue)));
            _contractName = name;
            return this;
        }

        /// <summary>
        /// Add metadata to the export.
        /// </summary>
        /// <param name="key">Metadata key.</param>
        /// <param name="value">Metadata value.</param>
        /// <returns>Builder for additional configuration.</returns>
        public ExportConfigurationBuilder WithMetadata(string key, object value)
        {
            _metadata.Add(key, value);
            return this;
        }

        /// <summary>
        /// Add metadata to the export.
        /// </summary>
        /// <param name="metadata">Metadata.</param>
        /// <returns>Builder for additional configuration.</returns>
        public ExportConfigurationBuilder WithMetadata(IEnumerable<KeyValuePair<string, object>> metadata)
        {
            foreach (var m in metadata)
                _metadata.Add(m);

            return this;
        }
    }
}
