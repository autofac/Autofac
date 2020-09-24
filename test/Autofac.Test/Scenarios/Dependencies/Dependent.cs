// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Test.Scenarios.Dependencies
{
    public class Dependent
    {
        public object TheObject { get; private set; }

        public string TheString { get; private set; }

        public Dependent(object o, string s)
        {
            TheObject = o;
            TheString = s;
        }
    }
}
