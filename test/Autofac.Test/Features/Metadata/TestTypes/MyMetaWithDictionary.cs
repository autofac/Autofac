// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Test.Features.Metadata.TestTypes;

[SuppressMessage("CA1711", "CA1711", Justification = "Naming helps clarify the purpose of the object for test consumers.")]
public class MyMetaWithDictionary
{
    public MyMetaWithDictionary(IDictionary<string, object> metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        TheName = (string)metadata["Name"];
    }

    public string TheName { get; set; }
}
