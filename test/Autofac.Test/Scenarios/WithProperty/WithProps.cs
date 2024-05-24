// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Test.Scenarios.WithProperty;

public class WithProps
{
    public string A { get; set; }

    public bool B { get; set; }

    [SuppressMessage("SA1401", "SA1401", Justification = "Public field handles a specific test case.")]
    [SuppressMessage("CA1051", "CA1051", Justification = "Public field handles a specific test case.")]
    public string _field;
}
