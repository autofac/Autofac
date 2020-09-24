// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
        /// <summary>
        /// Initializes a new instance of the <see cref="LightweightAdapterActivatorData"/> class.
        /// </summary>
        /// <param name="fromService">The service that will be adapted from.</param>
        /// <param name="adapter">The adapter function.</param>
        public LightweightAdapterActivatorData(
            Service fromService,
            Func<IComponentContext, IEnumerable<Parameter>, object, object> adapter)
        {
            FromService = fromService;
            Adapter = adapter;
        }

        /// <summary>
        /// Gets the adapter function.
        /// </summary>
        public Func<IComponentContext, IEnumerable<Parameter>, object, object> Adapter { get; }

        /// <summary>
        /// Gets the service to be adapted from.
        /// </summary>
        public Service FromService { get; }
    }
}