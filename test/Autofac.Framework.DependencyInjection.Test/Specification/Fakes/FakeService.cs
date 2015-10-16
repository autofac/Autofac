// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Framework.DependencyInjection.Tests.Fakes
{
    public class FakeService : IFakeEveryService, IDisposable
    {
        public FakeService()
        {
            Message = "FakeServiceSimpleMethod";
        }

        public bool Disposed { get; private set; }

        public string Message { get; set; }

        public string SimpleMethod()
        {
            return Message;
        }

        public void Dispose()
        {
            Disposed = true;
        }
    }
}
