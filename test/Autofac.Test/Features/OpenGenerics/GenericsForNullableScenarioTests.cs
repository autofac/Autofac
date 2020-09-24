// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Test.Util;
using Xunit;

namespace Autofac.Test.Features.OpenGenerics
{
    public class GenericsForNullableScenarioTests
    {
        public interface IItemProducer<T>
        {
        }

        public class NullableProducer<T> : IItemProducer<T?>
            where T : struct
        {
        }

        private readonly IContainer _container;

        public GenericsForNullableScenarioTests()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(NullableProducer<>)).As(typeof(IItemProducer<>));
            _container = builder.Build();
        }

        [Fact]
        public void TheServiceIsAvailable()
        {
            Assert.True(_container.IsRegistered<IItemProducer<byte?>>());
        }

        [Fact]
        public void TheImplementationTypeParametersAreMapped()
        {
            var np = _container.Resolve<IItemProducer<byte?>>();
            Assert.IsType<NullableProducer<byte>>(np);
        }

        [Fact]
        public void IncompatibleTypeParametersAreIgnored()
        {
            Assert.False(_container.IsRegistered<IItemProducer<byte>>());
        }
    }
}
