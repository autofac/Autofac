// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Specification.Test.Features.PropertyInjection;

public class CtorWithValueParameter
{
    public delegate CtorWithValueParameter Factory(string value);

    // The testing with this class has to do with a constructor
    // parameter that is named `value` - this property doesn't
    // need to be filled in, it just needs to exist and not be
    // a simple `object` or `string` or something.
    public HasMixedVisibilityProperties Dummy { get; set; }

    public CtorWithValueParameter(string value)
    {
    }
}
