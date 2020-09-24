// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Autofac.Core.Activators.Reflection;
using Autofac.Test.Util;
using Xunit;

namespace Autofac.Test.Core.Activators.Reflection
{
    public class DefaultValueParameterTests
    {
        public class HasDefaultValues
        {
            public HasDefaultValues(string s, string t = "Hello", DateTime dt = default, Guid guid = default)
            {
            }
        }

        private static ParameterInfo GetTestParameter(string name)
        {
            return typeof(HasDefaultValues).GetConstructors().Single()
                .GetParameters().Where(pi => pi.Name == name).Single();
        }

        private static ParameterInfo GetDynamicBuildParameter(int index)
        {
            var builder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString("N")), AssemblyBuilderAccess.Run).DefineDynamicModule(Guid.NewGuid().ToString("N"));
            var type = builder.DefineType("HasDefaultValues", TypeAttributes.Public);

            var constructorBuilder = type.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(string), typeof(string) });
            constructorBuilder.DefineParameter(2, ParameterAttributes.HasDefault, "t").SetConstant("Hello");
            var il = constructorBuilder.GetILGenerator();
            il.Emit(OpCodes.Ret);

            var typeInfo = type.CreateTypeInfo();
            return typeInfo.GetConstructors().Single().GetParameters()[index];
        }

        private static ParameterInfo GetDynamicMethodParameter()
        {
            var method = new DynamicMethod("x", typeof(void), new Type[] { typeof(string) });
            var il = method.GetILGenerator();
            il.Emit(OpCodes.Ret);
            var delegateMethod = method.CreateDelegate(typeof(Action<string>));
            return delegateMethod.GetMethodInfo().GetParameters()[0];
        }

        [Fact]
        public void DoesNotProvideValueWhenNoDefaultAvailable()
        {
            var dvp = new DefaultValueParameter();
            var dp = GetTestParameter("s").DefaultValue;
            Assert.False(dvp.CanSupplyValue(GetTestParameter("s"), new ContainerBuilder().Build(), out Func<object> vp));
        }

        [Fact]
        public void ProvidesValueWhenDefaultInitialiserPresent()
        {
            var dvp = new DefaultValueParameter();
            var u = GetTestParameter("t");
            var dp = u.DefaultValue;
            Assert.True(dvp.CanSupplyValue(u, new ContainerBuilder().Build(), out Func<object> vp));
            Assert.Equal("Hello", vp());
        }

        [Fact]
        public void ProvidesValueWhenDefaultDateTime()
        {
            var dvp = new DefaultValueParameter();
            var u = GetTestParameter("guid");
            Assert.True(dvp.CanSupplyValue(u, new ContainerBuilder().Build(), out Func<object> vp));
            Assert.Equal(default(Guid), vp());
        }

        [Fact]
        public void ProvidesValueWhenDefaultStructure()
        {
            var dvp = new DefaultValueParameter();
            var u = GetTestParameter("dt");
            Assert.True(dvp.CanSupplyValue(u, new ContainerBuilder().Build(), out Func<object> vp));
            Assert.Equal(default(DateTime), vp());
        }

        [Fact]
        public void DoesNotProvideValueWhenNoDefaultAvailableInDynamicAssembly()
        {
            var dvp = new DefaultValueParameter();

            Assert.False(dvp.CanSupplyValue(GetDynamicBuildParameter(0), new ContainerBuilder().Build(), out Func<object> vp));
        }

        [Fact]
        public void ProvidesValueWhenDefaultInitialiserPresentInDynamicAssembly()
        {
            var dvp = new DefaultValueParameter();

            Assert.True(dvp.CanSupplyValue(GetDynamicBuildParameter(1), new ContainerBuilder().Build(), out Func<object> vp));
            Assert.Equal("Hello", vp());
        }

        [Fact]
        public void DoesNotProvideValueInDynamicMethod()
        {
            var dvp = new DefaultValueParameter();

            Assert.False(dvp.CanSupplyValue(GetDynamicMethodParameter(), new ContainerBuilder().Build(), out Func<object> vp));
        }
    }
}
