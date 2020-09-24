// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Autofac.Specification.Test.Features.PropertyInjection
{
    public class HasWriteOnlyProperty
    {
        private string _val;

        public string Val
        {
            set
            {
                _val = value;
            }
        }

        public string GetVal()
        {
            return _val;
        }
    }
}
