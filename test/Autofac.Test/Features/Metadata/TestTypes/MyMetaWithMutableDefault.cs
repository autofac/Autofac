// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;

namespace Autofac.Test.Features.Metadata.TestTypes;

public class MyMetaWithMutableDefault
{
    [DefaultValue(new[] { 1, 2 })]
    [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "The array is the mutable default under test.")]
    public int[]? TheValues
    {
        get; set;
    }
}
