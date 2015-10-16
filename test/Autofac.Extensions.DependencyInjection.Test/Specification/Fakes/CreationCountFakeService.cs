// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;

namespace Microsoft.Extensions.DependencyInjection.Tests.Fakes
{
    public class CreationCountFakeService
    {
        private static int _instanceCount;

        private int _instanceId;

        public CreationCountFakeService(IFakeService dependency)
        {
            _instanceCount++;
            _instanceId = _instanceCount;
        }

        public static int InstanceCount
        {
            get
            {
                return _instanceCount;
            }
            set
            {
                _instanceCount = value;
            }
        }

        public int InstanceId
        {
            get
            {
                return _instanceId;
            }
        }
    }
}