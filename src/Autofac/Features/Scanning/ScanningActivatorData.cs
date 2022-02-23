// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;

namespace Autofac.Features.Scanning;

/// <summary>
/// Activation data for types located by scanning assemblies.
/// </summary>
public class ScanningActivatorData : BaseScanningActivatorData<ConcreteReflectionActivatorData, SingleRegistrationStyle>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScanningActivatorData"/> class.
    /// </summary>
    public ScanningActivatorData()
        : base(new List<Action<Type, IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>>>())
    {
    }
}
