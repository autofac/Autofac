using System;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Autofac.Specification.Test.Features.PropertyInjection;
using Xunit;

namespace Autofac.Specification.Test.Features
{
    public class PropertyInjectionTests
    {
        public enum SimpleEnumeration
        {
            A,
            B,
        }

        [Fact]
        public void EnumPropertiesCanBeAutowired()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<EnumProperty>().PropertiesAutowired();
            builder.Register(c => SimpleEnumeration.B);
            var container = builder.Build();
            var withE = container.Resolve<EnumProperty>();
            Assert.Equal(SimpleEnumeration.B, withE.Value);
        }

        [Fact]
        public void InjectPropertiesOverwritesSetProperties()
        {
            const string str = "test";

            var cb = new ContainerBuilder();
            cb.RegisterInstance(str);
            var c = cb.Build();

            var obj = new HasPublicSetterWithDefaultValue();
            c.InjectProperties(obj);
            Assert.Equal(str, obj.Val);
        }

        [Fact]
        public void InjectPropertiesWithDelegateSelectorAllowsPrivateSet()
        {
            const string str = "test";

            var cb = new ContainerBuilder();
            cb.RegisterInstance(str);
            var c = cb.Build();

            var obj = new HasMixedVisibilityProperties();

            Assert.Null(obj.PublicString);
            Assert.Null(obj.PrivateStringAccessor());
            c.InjectProperties(obj, new DelegatePropertySelector((p, _) => p.GetCustomAttributes<InjectAttribute>().Any()));
            Assert.Null(obj.PublicString);
            Assert.Equal(str, obj.PrivateStringAccessor());
        }

        [Fact]
        public void InjectPropertiesWithPropertySelectorAllowsPrivateSet()
        {
            const string str = "test";

            var cb = new ContainerBuilder();
            cb.RegisterInstance(str);
            var c = cb.Build();

            var obj = new HasMixedVisibilityProperties();

            Assert.Null(obj.PublicString);
            Assert.Null(obj.PrivateStringAccessor());
            c.InjectProperties(obj, new InjectAttributePropertySelector());
            Assert.Null(obj.PublicString);
            Assert.Equal(str, obj.PrivateStringAccessor());
        }

        [Fact]
        public void InjectUnsetPropertiesSkipsSetProperties()
        {
            const string str = "test";

            var cb = new ContainerBuilder();
            cb.RegisterInstance(str);
            var c = cb.Build();

            var obj = new HasPublicSetter()
            {
                Val = "set"
            };

            c.InjectUnsetProperties(obj);
            Assert.Equal("set", obj.Val);
        }

        [Fact]
        public void InjectUnsetPropertiesUsesPublicOnly()
        {
            const string str = "test";

            var cb = new ContainerBuilder();
            cb.RegisterInstance(str);
            var c = cb.Build();

            var obj = new HasMixedVisibilityProperties();

            Assert.Null(obj.PublicString);
            Assert.Null(obj.PrivateStringAccessor());
            c.InjectUnsetProperties(obj);
            Assert.Equal(str, obj.PublicString);
            Assert.Null(obj.PrivateStringAccessor());
        }

        [Fact]
        public void PropertiesAutowiredCanSetBaseClassProperties()
        {
            // Issue #2 from Autofac.Configuration - Ensure properties in base classes can be set by config.
            var val = "Value";

            var builder = new ContainerBuilder();
            builder.RegisterInstance(val);
            builder.RegisterType<HasPublicSetterDerived>().PropertiesAutowired();

            var container = builder.Build();

            var instance = container.Resolve<HasPublicSetterDerived>();

            Assert.NotNull(instance);
            Assert.Equal(val, instance.Val);
        }

        [Fact]
        public void PropertiesAutowiredDoesNotSetNonPublicSetter()
        {
            var val = "Value";

            var builder = new ContainerBuilder();
            builder.RegisterInstance(val);
            builder.RegisterType<HasProtectedSetterWithDefaultValue>().PropertiesAutowired();

            var container = builder.Build();

            var instance = container.Resolve<HasProtectedSetterWithDefaultValue>();

            Assert.NotNull(instance);
            Assert.Equal("Default", instance.Val);
        }

        [Fact]
        public void PropertiesAutowiredOverwritesSetProperties()
        {
            var val = "Value";

            var builder = new ContainerBuilder();
            builder.RegisterInstance(val);
            builder.RegisterType<HasPublicSetterWithDefaultValue>().PropertiesAutowired();

            var container = builder.Build();

            var instance = container.Resolve<HasPublicSetterWithDefaultValue>();

            Assert.NotNull(instance);
            Assert.Equal(val, instance.Val);
        }

        [Fact]
        public void PropertiesAutowiredSetsUnsetPublicSetter()
        {
            var val = "Value";

            var builder = new ContainerBuilder();
            builder.RegisterInstance(val);
            builder.RegisterType<HasPublicSetter>().PropertiesAutowired();
            var container = builder.Build();
            var instance = container.Resolve<HasPublicSetter>();
            Assert.NotNull(instance);
            Assert.Equal(val, instance.Val);
        }

        [Fact]
        public void PropertiesAutowiredSetsWriteOnlyPublicProperty()
        {
            var val = "Value";

            var builder = new ContainerBuilder();
            builder.RegisterInstance(val);
            builder.RegisterType<HasWriteOnlyProperty>().PropertiesAutowired();

            var container = builder.Build();
            var instance = container.Resolve<HasWriteOnlyProperty>();

            Assert.NotNull(instance);
            Assert.Equal(val, instance.GetVal());
        }

        [Fact]
        public void PropertiesAutowiredUsingDelegateSelector()
        {
            const string str = "test";

            var cb = new ContainerBuilder();
            cb.Register(_ => new HasMixedVisibilityProperties())
                .PropertiesAutowired(new DelegatePropertySelector((p, _) => p.GetCustomAttributes<InjectAttribute>().Any()));
            cb.RegisterInstance(str);

            var c = cb.Build();
            var obj = c.Resolve<HasMixedVisibilityProperties>();
            Assert.Null(obj.PublicString);
            Assert.Equal(str, obj.PrivateStringAccessor());
        }

        [Fact]
        public void PropertiesAutowiredUsingInlineDelegate()
        {
            const string str = "test";

            var cb = new ContainerBuilder();
            cb.RegisterType<HasMixedVisibilityProperties>()
                .PropertiesAutowired((propInfo, instance) => true);
            cb.RegisterInstance(str); // Must register, otherwise delegate won't be called

            var c = cb.Build();
            var obj = c.Resolve<HasMixedVisibilityProperties>();

            Assert.Equal(str, obj.PublicString);
            Assert.Equal(str, obj.PrivateStringAccessor());
        }

        [Fact]
        public void PropertiesAutowiredUsingPropertySelector()
        {
            const string str = "test";

            var cb = new ContainerBuilder();
            cb.Register(_ => new HasMixedVisibilityProperties())
                .PropertiesAutowired(new InjectAttributePropertySelector());
            cb.RegisterInstance(str);

            var c = cb.Build();
            var obj = c.Resolve<HasMixedVisibilityProperties>();
            Assert.Null(obj.PublicString);
            Assert.Equal(str, obj.PrivateStringAccessor());
        }

        [Fact]
        public void PropertiesNotSetIfNotSpecified()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<HasPublicSetter>();
            var container = builder.Build();
            var instance = container.Resolve<HasPublicSetter>();
            Assert.NotNull(instance);
            Assert.Null(instance.Val);
        }

        public class EnumProperty
        {
            public SimpleEnumeration Value { get; set; }
        }
    }
}
