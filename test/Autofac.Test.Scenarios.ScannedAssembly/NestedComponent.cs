// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;

namespace Autofac.Test.Scenarios.ScannedAssembly
{
    public class NestedComponent
    {
        private class PrivateComponent
        {
        }

        internal class InternalComponent
        {
        }

        public class PublicComponent
        {
        }
    }
}
