// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Features.Decorators;

namespace Autofac.Test.Features.Decorators;

public class DecoratorContextTests
{
    [Fact]
    public void CreateSetsContextToPreDecoratedState()
    {
        const string ImplementationInstance = "Initial";

        var context = DecoratorContext.Create(Factory.CreateEmptyContext(), typeof(string), typeof(string), ImplementationInstance);

        Assert.Equal(typeof(string), context.ServiceType);
        Assert.Equal(typeof(string), context.ImplementationType);
        Assert.Equal(ImplementationInstance, context.CurrentInstance);
        Assert.Empty(context.AppliedDecoratorTypes);
        Assert.Empty(context.AppliedDecorators);
    }

    [Fact]
    public void UpdateAddsDecoratorStateToContext()
    {
        const string ImplementationInstance = "Initial";
        var context = DecoratorContext.Create(Factory.CreateEmptyContext(), typeof(string), typeof(string), ImplementationInstance);

        const string DecoratorA = "DecoratorA";
        context = context.UpdateContext(DecoratorA);

        Assert.Equal(DecoratorA, context.CurrentInstance);
        Assert.Equal(context.AppliedDecoratorTypes, new[] { typeof(string) });
        Assert.Equal(context.AppliedDecorators, new[] { DecoratorA });

        const string DecoratorB = "DecoratorB";
        context = context.UpdateContext(DecoratorB);

        Assert.Equal(DecoratorB, context.CurrentInstance);
        Assert.Equal(context.AppliedDecoratorTypes, new[] { typeof(string), typeof(string) });
        Assert.Equal(context.AppliedDecorators, new[] { DecoratorA, DecoratorB });
    }
}
