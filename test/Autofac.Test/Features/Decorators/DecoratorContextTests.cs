// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Features.Decorators;
using Xunit;

namespace Autofac.Test.Features.Decorators
{
    public class DecoratorContextTests
    {
        [Fact]
        public void CreateSetsContextToPreDecoratedState()
        {
            const string implementationInstance = "Initial";

            var context = DecoratorContext.Create(typeof(string), typeof(string), implementationInstance);

            Assert.Equal(typeof(string), context.ServiceType);
            Assert.Equal(typeof(string), context.ImplementationType);
            Assert.Equal(implementationInstance, context.CurrentInstance);
            Assert.Empty(context.AppliedDecoratorTypes);
            Assert.Empty(context.AppliedDecorators);
        }

        [Fact]
        public void UpdateAddsDecoratorStateToContext()
        {
            const string implementationInstance = "Initial";
            var context = DecoratorContext.Create(typeof(string), typeof(string), implementationInstance);

            const string decoratorA = "DecoratorA";
            context = context.UpdateContext(decoratorA);

            Assert.Equal(decoratorA, context.CurrentInstance);
            Assert.Equal(context.AppliedDecoratorTypes, new[] { typeof(string) });
            Assert.Equal(context.AppliedDecorators, new[] { decoratorA });

            const string decoratorB = "DecoratorB";
            context = context.UpdateContext(decoratorB);

            Assert.Equal(decoratorB, context.CurrentInstance);
            Assert.Equal(context.AppliedDecoratorTypes, new[] { typeof(string), typeof(string) });
            Assert.Equal(context.AppliedDecorators, new[] { decoratorA, decoratorB });
        }
    }
}
