// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Specification.Test.Features.PropertyInjection;

public class HasWriteOnlyProperty
{
    private string _val;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("CA1044", "CA1044", Justification = "Handles a specific test case for a write-only property.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CA1721", "CA1721", Justification = "Handles a specific test case for a write-only property.")]
    public string Val
    {
        set
        {
            _val = value;
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("CA1024", "CA1024", Justification = "Handles a specific test case for a write-only property.")]
    public string GetVal()
    {
        return _val;
    }
}
