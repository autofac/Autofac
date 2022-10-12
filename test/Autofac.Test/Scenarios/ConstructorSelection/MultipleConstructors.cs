// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Test.Scenarios.ConstructorSelection;

public class MultipleConstructors
{
    public MultipleConstructors(object o, string s)
    {
    }

    public MultipleConstructors(object o)
    {
    }
}
