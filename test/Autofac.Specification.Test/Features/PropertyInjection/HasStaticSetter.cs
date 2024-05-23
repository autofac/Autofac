// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Specification.Test.Features.PropertyInjection;

[SuppressMessage("CA1052", "CA1052", Justification = "Handles a specific test scenario of a non-static class with a static property.")]
public class HasStaticSetter
{
    public static string Val { get; set; }
}
