// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using Autofac.Core;
using Xunit;

namespace Autofac.Test.Core
{
    // ReSharper disable ClassNeverInstantiated.Local, UnusedParameter.Local
    public class DependencyResolutionExceptionTests
    {
        public class A
        {
            public const string Message = "This is the original exception.";

            public A()
            {
                throw new InvalidOperationException(Message);
            }
        }

        public class B
        {
            public B(A a)
            {
            }
        }

        public class C
        {
            public C(B b)
            {
            }
        }

        [Fact]
        public void ExceptionMessageUnwrapsNestedResolutionFailures()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<A>();
            builder.Register(c => new B(c.Resolve<A>()));
            builder.RegisterType<C>();

            Exception ex;
            using (var container = builder.Build())
            {
                ex = Assert.Throws<DependencyResolutionException>(() => container.Resolve<C>());
            }

            var n = GetType().FullName;
            Assert.Equal($"An exception was thrown while activating {n}+C -> λ:{n}+B -> {n}+A.", ex.Message);

            var inner = ex.InnerException;
            Assert.IsType<DependencyResolutionException>(inner);
            Assert.Equal("An exception was thrown while invoking the constructor 'Void .ctor()' on type 'A'.", inner.Message);

            Assert.IsType<InvalidOperationException>(inner.InnerException);
            Assert.Equal(A.Message, inner.InnerException.Message);
        }

        [Serializable]
        public class CustomDependencyResolutionException : DependencyResolutionException
        {
            public int Value { get; }

            public CustomDependencyResolutionException(int value)
                : base(null)
            {
                Value = value;
            }

            protected CustomDependencyResolutionException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
                if (info == null)
                {
                    throw new ArgumentNullException(nameof(info));
                }

                Value = info.GetInt32(nameof(Value));
            }

            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                base.GetObjectData(info, context);

                info.AddValue(nameof(Value), 123);
            }
        }

        [Fact]
        public void SupportCustomRuntimeSerialization()
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, new CustomDependencyResolutionException(123));

                stream.Position = 0;
                var exception = (CustomDependencyResolutionException)formatter.Deserialize(stream);

                Assert.Equal(123, exception.Value);
            }
        }
    }
}
