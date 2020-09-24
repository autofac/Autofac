// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Autofac.Features.Decorators;
using Xunit;

namespace Autofac.Test.Features.Decorators
{
    public class DecoratorServiceTests
    {
        [Fact]
        public void MustBeConstructedWithServiceType()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new DecoratorService(null));
            Assert.Equal("serviceType", exception.ParamName);
        }

        [Fact]
        public void ConditionDefaultsToTrueWhenNotProvided()
        {
            var service = new DecoratorService(typeof(string));

            var context = DecoratorContext.Create(typeof(string), typeof(string), "A");

            Assert.True(service.Condition(context));
        }

        [Fact]
        public void ServicesAreEqualWhenServiceTypeMatches()
        {
            var service1 = new DecoratorService(typeof(string));
            var service2 = new DecoratorService(typeof(string));

            Assert.Equal(service1, service2);
        }

        [Fact]
        public void ServicesGetHashCodeAreEqualWhenServiceTypeMatches()
        {
            var service1 = new DecoratorService(typeof(string));
            var service2 = new DecoratorService(typeof(string));

            Assert.Equal(service1.GetHashCode(), service2.GetHashCode());
        }

        [Fact]
        public void ServicesWithSameServiceTypeButDifferentConditionsAreEqual()
        {
            var service1 = new DecoratorService(typeof(string), ctx => (string)ctx.CurrentInstance == "A");
            var service2 = new DecoratorService(typeof(string), ctx => (string)ctx.CurrentInstance == "B");

            Assert.Equal(service1, service2);
        }
    }
}
