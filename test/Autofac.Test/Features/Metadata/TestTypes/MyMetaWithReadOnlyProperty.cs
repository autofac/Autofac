// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Test.Features.Metadata.TestTypes;

public class MyMetaWithReadOnlyProperty
{
    public int TheInt { get; set; }

    public string ReadOnly
    {
        get
        {
            return "Foo";
        }
    }
}
