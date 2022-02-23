// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;

namespace Autofac.Features.Scanning;

/// <summary>
/// Activation data for open generic types located by scanning assemblies.
/// </summary>
public class OpenGenericScanningActivatorData : BaseScanningActivatorData<ReflectionActivatorData, DynamicRegistrationStyle>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OpenGenericScanningActivatorData"/> class.
    /// </summary>
    public OpenGenericScanningActivatorData()
        : base(new List<Action<Type, IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>>>())
    {
    }
}
