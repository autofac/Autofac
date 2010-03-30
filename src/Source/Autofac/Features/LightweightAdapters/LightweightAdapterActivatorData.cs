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