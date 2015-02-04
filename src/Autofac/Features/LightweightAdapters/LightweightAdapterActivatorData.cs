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
using Autofac.Core;

namespace Autofac.Features.LightweightAdapters
{
    /// <summary>
    /// Describes the basic requirements for generating a lightweight adapter.
    /// </summary>
    public class LightweightAdapterActivatorData
    {
        readonly Service _fromService;
        readonly Func<IComponentContext, IEnumerable<Parameter>, object, object> _adapter;

        /// <summary>
        /// Create an instance of <see cref="LightweightAdapterActivatorData"/>.
        /// </summary>
        /// <param name="fromService">The service that will be adapted from.</param>
        /// <param name="adapter">The adapter function.</param>
        public LightweightAdapterActivatorData(
            Service fromService,
            Func<IComponentContext, IEnumerable<Parameter>, object, object> adapter)
        {
            _fromService = fromService;
            _adapter = adapter;
        }

        /// <summary>
        /// The adapter function.
        /// </summary>
        public Func<IComponentContext, IEnumerable<Parameter>, object, object> Adapter
        {
            get { return _adapter; }
        }

        /// <summary>
        /// The service to be adapted from.
        /// </summary>
        public Service FromService
        {
            get { return _fromService; }
        }
    }
}