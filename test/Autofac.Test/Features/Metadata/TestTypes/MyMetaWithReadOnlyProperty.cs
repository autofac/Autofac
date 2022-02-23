// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Autofac.Test.Features.Metadata.TestTypes
{
    public class MyMetaWithReadOnlyProperty
    {
        public int TheInt { get; set; }

        [SuppressMessage("CA1822", "CA1822", Justification = "Property needs to be instance for testing.")]
        public string ReadOnly
        {
            get
            {
                return "Foo";
            }
        }
    }
}
