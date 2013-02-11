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
using Autofac.Builder;
using Autofac.Features.Scanning;

namespace Autofac.Extras.Attributed
{
    /// <summary>
    /// Extends registration syntax for attribute scenarios.
    /// </summary>
    public static class AutofacAttributeExtensions
    {
        /// <summary>
        /// This method can be invoked with the assembly scanner to register metadata that is declared loosely using
        /// attributes marked with the MetadataAttributeAttribute. All of the marked attributes are used together to create
        /// a common set of dictionary values that constitute the metadata on the type.
        /// </summary>
        /// <typeparam name="TLimit">The type of the limit</typeparam>
        /// <typeparam name="TScanningActivatorData">activator data type</typeparam>
        /// <typeparam name="TRegistrationStyle">registration style type</typeparam>
        /// <param name="builder">container builder</param>
        /// <returns>registration builder allowing the registration to be configured</returns>
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            WithAttributedMetadata<TLimit, TScanningActivatorData, TRegistrationStyle>
                        (this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> builder)
                                        where TScanningActivatorData : ScanningActivatorData
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            builder.ActivatorData.ConfigurationActions.Add(
                (t, rb) => rb.WithMetadata(MetadataHelper.GetMetadata(t)));

            return builder;
        }


        /// <summary>
        /// This method can be invoked with the assembly scanner to register strongly typed metadata attributes. The
        /// attributes are scanned for one that is derived from the metadata interface.  If one is found, the metadata
        /// contents are extracted and registered with the instance registration
        /// </summary>
        /// <typeparam name="TMetadata">metadata type to search for</typeparam>
        /// <param name="builder">container builder</param>
        /// <returns>registration builder allowing the registration to be configured</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            WithAttributedMetadata<TMetadata>(this IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            builder.ActivatorData.ConfigurationActions.Add(
                (t, rb) => rb.WithMetadata(MetadataHelper.GetMetadata<TMetadata>(t)));

            return builder;
        }
    }
}
