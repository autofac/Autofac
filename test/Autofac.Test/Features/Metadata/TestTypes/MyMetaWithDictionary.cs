﻿// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Test.Features.Metadata.TestTypes;

public class MyMetaWithDictionary
{
    public MyMetaWithDictionary(IDictionary<string, object> metadata)
    {
        TheName = (string)metadata["Name"];
    }

    public string TheName { get; set; }
}
