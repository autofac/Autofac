// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Xunit;

namespace Autofac.Specification.Test.Registration
{
    public class OpenGenericWithMultipleInterfacesTests
    {
        private interface IHandler<in T>
        {
            void Handle(T input);
        }

        private class FirstInterfaceIsDocument<TDocument> :
            IHandler<TDocument>,
            IHandler<List<TDocument>>
        {
            public void Handle(TDocument input) => throw new NotImplementedException();

            public void Handle(List<TDocument> input) => throw new NotImplementedException();
        }

        private class FirstInterfaceIsListOfDocument<TDocument> :
            IHandler<List<TDocument>>,
            IHandler<TDocument>
        {
            public void Handle(TDocument input) => throw new NotImplementedException();

            public void Handle(List<TDocument> input) => throw new NotImplementedException();
        }

        [Theory]
        [InlineData(typeof(FirstInterfaceIsDocument<>), typeof(IHandler<object>), typeof(FirstInterfaceIsDocument<object>), false)]
        [InlineData(typeof(FirstInterfaceIsDocument<>), typeof(IHandler<object>), typeof(FirstInterfaceIsDocument<object>), true)]
        [InlineData(typeof(FirstInterfaceIsDocument<>), typeof(IHandler<List<object>>), typeof(FirstInterfaceIsDocument<List<object>>), false)]
        [InlineData(typeof(FirstInterfaceIsDocument<>), typeof(IHandler<List<object>>), typeof(FirstInterfaceIsDocument<List<object>>), true)]
        [InlineData(typeof(FirstInterfaceIsListOfDocument<>), typeof(IHandler<object>), typeof(FirstInterfaceIsListOfDocument<object>), false)]
        [InlineData(typeof(FirstInterfaceIsListOfDocument<>), typeof(IHandler<object>), typeof(FirstInterfaceIsListOfDocument<object>), true)]
        [InlineData(typeof(FirstInterfaceIsListOfDocument<>), typeof(IHandler<List<object>>), typeof(FirstInterfaceIsListOfDocument<List<object>>), false)]
        [InlineData(typeof(FirstInterfaceIsListOfDocument<>), typeof(IHandler<List<object>>), typeof(FirstInterfaceIsListOfDocument<List<object>>), true)]
        public void CorrectClosedGenericTypeResolvedWhenMultipleInterfacesImplemented(
            Type openGenericType,
            Type serviceType,
            Type resolvedType,
            bool registerAsImplementedInterfaces = false)
        {
            var container = CreateContainerWithOpenGeneric(openGenericType, registerAsImplementedInterfaces);

            var instance = container.Resolve(serviceType);

            Assert.IsType(resolvedType, instance);
        }

        private static IContainer CreateContainerWithOpenGeneric(Type type, bool registerAsImplementedInterfaces = false)
        {
            var builder = new ContainerBuilder();

            if (registerAsImplementedInterfaces)
            {
                builder.RegisterGeneric(type).AsImplementedInterfaces();
            }
            else
            {
                builder.RegisterGeneric(type).As(typeof(IHandler<>));
            }

            return builder.Build();
        }
    }
}
