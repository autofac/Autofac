// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2013 Autofac Contributors
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
using Autofac.Core;

namespace Autofac.Extras.Attributed
{
    /// <summary>
    /// this module will scan all registrations for metadata and associate them if found
    /// </summary>
    public class AttributedMetadataModule : Module
    {
        /// <summary>
        /// Override to attach module-specific functionality to a
        /// component registration.
        /// </summary>
        /// <remarks>This method will be called for all existing <i>and future</i> component
        /// registrations - ordering is not important.</remarks>
        /// <param name="componentRegistry">The component registry.</param>
        /// <param name="registration">The registration to attach functionality to.</param>
        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException("registration");
            }
            foreach (var property in MetadataHelper.GetMetadata(registration.Activator.LimitType))
                registration.Metadata.Add(property);
        }
    }
}
