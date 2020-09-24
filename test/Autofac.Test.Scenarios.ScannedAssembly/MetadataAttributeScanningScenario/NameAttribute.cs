// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Autofac.Test.Scenarios.ScannedAssembly.MetadataAttributeScanningScenario
{
    public class NameAttribute : Attribute, IHaveName
    {
        public NameAttribute(string name)
        {
            Name = name ?? throw new ArgumentNullException("name");
        }

        public string Name { get; }
    }
}
