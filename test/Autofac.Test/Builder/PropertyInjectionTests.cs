using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Xunit;

namespace Autofac.Test.Builder
{
    public class PropertyInjectionTests
    {
        [Fact]
        public void NullCheckTests()
        {
            var ctx = new ContainerBuilder().Build();
            var instance = new object();
            var propertySelector = new DefaultPropertySelector(true);
            var parameters = new Parameter[0];

            Assert.Throws<ArgumentNullException>(() => AutowiringPropertyInjector.InjectProperties(null, instance, propertySelector, parameters));
            Assert.Throws<ArgumentNullException>(() => AutowiringPropertyInjector.InjectProperties(ctx, null, propertySelector, parameters));
            Assert.Throws<ArgumentNullException>(() => AutowiringPropertyInjector.InjectProperties(ctx, instance, null, parameters));
            Assert.Throws<ArgumentNullException>(() => AutowiringPropertyInjector.InjectProperties(ctx, instance, propertySelector, null));
        }

        [Fact]
        public void PropertySpecifiedAsResolveParameterNoRegistrationPropertySpecified()
        {
            // Issue #289 tried to get parameters to work as passed in directly to resolve
            // but issue #789 found a problem with trying to do that. Now it's just
            // manual property injection that allows parameters.
            var builder = new ContainerBuilder();
            builder.RegisterType<ConstructorParamNotAttachedToProperty>().WithParameter(TypedParameter.From("ctor"));
            var container = builder.Build();

            var instance = container.Resolve<ConstructorParamNotAttachedToProperty>();
            Assert.Null(instance.Name);
            container.InjectProperties(instance, new NamedPropertyParameter("Name", "value"));
            Assert.Equal("value", instance.Name);
        }

        [Fact]
        public void PropertySpecifiedAsResolveParameterWhenAutowired()
        {
            // Issue #289 tried to get parameters to work as passed in directly to resolve
            // but issue #789 found a problem with trying to do that. Now it's just
            // manual property injection that allows parameters.
            var builder = new ContainerBuilder();
            builder.RegisterType<ConstructorParamNotAttachedToProperty>().WithParameter(TypedParameter.From("ctor")).PropertiesAutowired();
            var container = builder.Build();

            var instance = container.Resolve<ConstructorParamNotAttachedToProperty>(new NamedPropertyParameter("Name", "value"));
            Assert.Equal("ctor", instance._id);
            Assert.Equal("value", instance.Name);
        }

        [Fact]
        public void PropertySpecifiedAsResolveParameterWhenAutowiredMayBeBothConstructorAndProperty()
        {
            // Issue #289 tried to get parameters to work as passed in directly to resolve
            // but issue #789 found a problem with trying to do that. Now it's just
            // manual property injection that allows parameters.
            var builder = new ContainerBuilder();
            builder.RegisterType<ConstructorParamNotAttachedToProperty>().PropertiesAutowired();
            var container = builder.Build();

            var instance = container.Resolve<ConstructorParamNotAttachedToProperty>(TypedParameter.From("test"));
            Assert.Equal("test", instance._id);
            Assert.Equal("test", instance.Name);
        }

        [Fact]
        public void SetterInjectionIgnoresArraysOfNullableValueTypes()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<HasNullableValueTypeArray>().PropertiesAutowired();
            var container = builder.Build();

            var instance = container.Resolve<HasNullableValueTypeArray>();

            Assert.Equal(new double?[] { null, 0.1, null }, instance.DoubleArray);
        }

        [Fact]
        public void SetterInjectionIgnoresArraysOfValueTypes()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<HasValueTypeArray>().PropertiesAutowired();
            var container = builder.Build();

            var instance = container.Resolve<HasValueTypeArray>();

            Assert.Equal(new byte[] { 1, 2, 3 }, instance.ByteArray);
        }

        [Fact]
        public void SetterInjectionIgnoresCollectionsOfNullableValueTypes()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<HasNullableValueTypeCollection>().PropertiesAutowired();
            var container = builder.Build();

            var instance = container.Resolve<HasNullableValueTypeCollection>();

            var expected = new ReadOnlyCollection<double?>(new double?[] { null, 0.1, null });
            Assert.Equal(expected, instance.DoubleCollectionInterface);
            Assert.Equal(expected, instance.DoubleCollection);
        }

        [Fact]
        public void SetterInjectionIgnoresCollectionsOfValueTypes()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<HasValueTypeCollection>().PropertiesAutowired();
            var container = builder.Build();

            var instance = container.Resolve<HasValueTypeCollection>();

            var expected = new Collection<byte> { 1, 2, 3 };
            Assert.Equal(expected, instance.ByteCollectionInterface);
            Assert.Equal(expected, instance.ByteCollection);
        }

        [Fact]
        public void SetterInjectionIgnoresListsOfNullableValueTypes()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<HasNullableValueTypeList>().PropertiesAutowired();
            var container = builder.Build();

            var instance = container.Resolve<HasNullableValueTypeList>();

            var expected = new List<double?> { null, 0.1, null };
            Assert.Equal(expected, instance.DoubleListInterface);
            Assert.Equal(expected, instance.DoubleList);
        }

        [Fact]
        public void SetterInjectionIgnoresListsOfValueTypes()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<HasValueTypeList>().PropertiesAutowired();
            var container = builder.Build();

            var instance = container.Resolve<HasValueTypeList>();

            var expected = new List<byte> { 1, 2, 3 };
            Assert.Equal(expected, instance.ByteListInterface);
            Assert.Equal(expected, instance.ByteList);
        }

        [Fact]
        public void SetterInjectionPrivateGet()
        {
            var val = "Value";

            var builder = new ContainerBuilder();
            builder.RegisterInstance(val);
            builder.RegisterType<SplitAccess>().PropertiesAutowired();

            var container = builder.Build();
            var instance = container.Resolve<SplitAccess>();

            Assert.NotNull(instance);
            Assert.True(instance.SetterCalled);
            Assert.False(instance.GetterCalled);
        }

        [Fact]
        public void TypedParameterForConstructorShouldNotAttachToProperty()
        {
            // Issue #789: If the parameters automatically flow from resolve
            // to property injection when PropertiesAutowired isn't specified
            // then properties get inadvertently resolved.
            var cb = new ContainerBuilder();
            cb.RegisterType<ConstructorParamNotAttachedToProperty>();
            var container = cb.Build();
            var resolved = container.Resolve<ConstructorParamNotAttachedToProperty>(TypedParameter.From("test"));
            Assert.Equal("test", resolved._id);
            Assert.Null(resolved.Name);
        }

        private class ConstructorParamNotAttachedToProperty
        {
            [SuppressMessage("SA1401", "SA1401")]
            public string _id = null;

            public ConstructorParamNotAttachedToProperty(string id)
            {
                this._id = id;
            }

            public string Name { get; set; }
        }

        public class HasNullableValueTypeArray
        {
            public HasNullableValueTypeArray()
            {
                this.DoubleArray = new double?[] { null, 0.1, null };
            }

            public double?[] DoubleArray { get; set; }
        }

        public class HasNullableValueTypeCollection
        {
            public HasNullableValueTypeCollection()
            {
                this.DoubleCollection = new ReadOnlyCollection<double?>(new double?[] { null, 0.1, null });
                this.DoubleCollectionInterface = this.DoubleCollection;
            }

            public ReadOnlyCollection<double?> DoubleCollection { get; set; }

            public IReadOnlyCollection<double?> DoubleCollectionInterface { get; set; }
        }

        public class HasNullableValueTypeList
        {
            public HasNullableValueTypeList()
            {
                this.DoubleList = new List<double?> { null, 0.1, null };
                this.DoubleListInterface = this.DoubleList;
            }

            public List<double?> DoubleList { get; set; }

            public IList<double?> DoubleListInterface { get; set; }
        }

        public class HasValueTypeArray
        {
            public HasValueTypeArray()
            {
                this.ByteArray = new byte[] { 1, 2, 3 };
            }

            public byte[] ByteArray { get; set; }
        }

        public class HasValueTypeCollection
        {
            public HasValueTypeCollection()
            {
                this.ByteCollection = new Collection<byte> { 1, 2, 3 };
                this.ByteCollectionInterface = this.ByteCollection;
            }

            public Collection<byte> ByteCollection { get; set; }

            public ICollection<byte> ByteCollectionInterface { get; set; }
        }

        public class HasValueTypeList
        {
            public HasValueTypeList()
            {
                this.ByteList = new List<byte> { 1, 2, 3 };
                this.ByteListInterface = this.ByteList;
            }

            public List<byte> ByteList { get; set; }

            public IList<byte> ByteListInterface { get; set; }
        }

        public class SplitAccess
        {
            public bool GetterCalled
            {
                get;
                set;
            }

            public bool SetterCalled
            {
                get;
                set;
            }

            public string Value
            {
                private get
                {
                    this.GetterCalled = true;
                    return null;
                }

                set
                {
                    this.SetterCalled = true;
                }
            }
        }
    }
}
