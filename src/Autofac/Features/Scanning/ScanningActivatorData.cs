// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Autofac.Builder;
using Autofac.Core.Registration;

namespace Autofac.Features.Scanning
{
    /// <summary>
    /// Activation data for types located by scanning assemblies.
    /// </summary>
    public class ScanningActivatorData : ReflectionActivatorData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScanningActivatorData"/> class.
        /// </summary>
        public ScanningActivatorData()
            : base(typeof(object))
        {
        }

        /// <summary>
        /// Gets the filters applied to the types from the scanned assembly.
        /// </summary>
        public ICollection<Func<Type, bool>> Filters { get; } = new List<Func<Type, bool>>();

        /// <summary>
        /// Gets the additional actions to be performed on the concrete type registrations.
        /// </summary>
        public ICollection<Action<Type, IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>>> ConfigurationActions { get; }
            = new List<Action<Type, IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>>>();

        /// <summary>
        /// Gets the actions to be called once the scanning operation is complete.
        /// </summary>
        public ICollection<Action<IComponentRegistryBuilder>> PostScanningCallbacks { get; } = new List<Action<IComponentRegistryBuilder>>();
    }
}
