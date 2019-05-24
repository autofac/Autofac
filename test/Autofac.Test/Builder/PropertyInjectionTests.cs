using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Xunit;

namespace Autofac.Test.Builder
{
    // It may or may not be interesting long-term to ignore value type arrays/lists
    // so the tests around that are not part of the specification.
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
    }
}
