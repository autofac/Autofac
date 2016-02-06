using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xunit;

namespace Autofac.Test.Builder
{
    public class PropertyInjectionTests
    {
        public class HasSetter
        {
            string _val;

            public string Val
            {
                get
                {
                    return _val;
                }
                set
                {
                    _val = value;
                }
            }
        }

        [Fact]
        public void SetterInjection()
        {
            var val = "Value";

            var builder = new ContainerBuilder();
            builder.RegisterInstance(val);
            builder.RegisterType<HasSetter>().PropertiesAutowired();

            var container = builder.Build();

            var instance = container.Resolve<HasSetter>();

            Assert.NotNull(instance);
            Assert.Equal(val, instance.Val);
        }

        [Fact]
        public void SetterInjectionUnset()
        {
            var val = "Value";

            var builder = new ContainerBuilder();
            builder.RegisterInstance(val);
            builder.RegisterType<HasSetter>().PropertiesAutowired();

            var container = builder.Build();

            var instance = container.Resolve<HasSetter>();

            Assert.NotNull(instance);
            Assert.Equal(val, instance.Val);
        }

        public class HasSetterWithValue
        {
            string _val = "Default";

            public string Val
            {
                get
                {
                    return _val;
                }
                set
                {
                    _val = value;
                }
            }
        }

        //[Fact]
        //public void SetterInjectionUnsetWithValue()
        //{
        //    var val = "Value";

        //    var builder = new ContainerBuilder();
        //    builder.Register(val);
        //    builder.Register<HasSetterWithValue>()
        //        .OnActivating(ActivatingHandler.InjectUnsetProperties);

        //    var container = builder.Build();

        //    var instance = container.Resolve<HasSetterWithValue>();

        //    Assert.NotNull(instance);
        //    Assert.Equal("Default", instance.Val);
        //}

        [Fact]
        public void SetterInjectionWithValue()
        {
            var val = "Value";

            var builder = new ContainerBuilder();
            builder.RegisterInstance(val);
            builder.RegisterType<HasSetterWithValue>().PropertiesAutowired();

            var container = builder.Build();

            var instance = container.Resolve<HasSetterWithValue>();

            Assert.NotNull(instance);
            Assert.Equal(val, instance.Val);
        }

        public class HasPropReadOnly
        {
            string _val = "Default";

            public string Val
            {
                get
                {
                    return _val;
                }
                protected set
                {
                    _val = value;
                }
            }
        }

        [Fact]
        public void SetterInjectionReadOnly()
        {
            var val = "Value";

            var builder = new ContainerBuilder();
            builder.RegisterInstance(val);
            builder.RegisterType<HasPropReadOnly>().PropertiesAutowired();

            var container = builder.Build();

            var instance = container.Resolve<HasPropReadOnly>();

            Assert.NotNull(instance);
            Assert.Equal("Default", instance.Val);
        }

        [Fact]
        public void SetterInjectionUnsetReadOnly()
        {
            var val = "Value";

            var builder = new ContainerBuilder();
            builder.RegisterInstance(val);
            builder.RegisterType<HasPropReadOnly>().PropertiesAutowired();

            var container = builder.Build();

            var instance = container.Resolve<HasPropReadOnly>();

            Assert.NotNull(instance);
            Assert.Equal("Default", instance.Val);
        }

        public class HasPropWriteOnly
        {
            string _val;

            public string Val
            {
                set
                {
                    _val = value;
                }
            }

            public string GetVal()
            {
                return _val;
            }
        }

        [Fact]
        public void SetterInjectionWriteOnly()
        {
            var val = "Value";

            var builder = new ContainerBuilder();
            builder.RegisterInstance(val);
            builder.RegisterType<HasPropWriteOnly>().PropertiesAutowired();

            var container = builder.Build();
            var instance = container.Resolve<HasPropWriteOnly>();

            Assert.NotNull(instance);
            Assert.Equal(val, instance.GetVal());
        }

        [Fact]
        public void SetterInjectionUnsetWriteOnly()
        {
            var val = "Value";

            var builder = new ContainerBuilder();
            builder.RegisterInstance(val);
            builder.RegisterType<HasPropWriteOnly>().PropertiesAutowired();

            var container = builder.Build();
            var instance = container.Resolve<HasPropWriteOnly>();

            Assert.NotNull(instance);
            Assert.Equal(val, instance.GetVal());
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
                    GetterCalled = true;
                    return null;
                }
                set
                {
                    SetterCalled = true;
                }
            }

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

        public class HasSetterDerived : HasSetterBase
        {
        }

        public class HasSetterBase
        {
            public string Val { get; set; }
        }

        [Fact]
        public void SetterInjectionBaseClassProperty()
        {
            // Issue #2 from Autofac.Configuration - Ensure properties in base classes can be set by config.
            var val = "Value";

            var builder = new ContainerBuilder();
            builder.RegisterInstance(val);
            builder.RegisterType<HasSetterDerived>().PropertiesAutowired();

            var container = builder.Build();

            var instance = container.Resolve<HasSetterDerived>();

            Assert.NotNull(instance);
            Assert.Equal(val, instance.Val);
        }


        public class Invokee
        {
            public int Param { get; set; }

            public void Method(int param)
            {
                Param = param;
            }
        }

        [Fact]
        public void EventFiredWithContainerScope()
        {
            var pval = 12;
            var builder = new ContainerBuilder();
            builder.RegisterType<Invokee>()
                .InstancePerLifetimeScope()
                .OnActivated(e => e.Instance.Method(pval));
            var container = builder.Build();
            var inner = container.BeginLifetimeScope();
            var invokee = inner.Resolve<Invokee>();
            Assert.Equal(pval, invokee.Param);
        }

        public class HasValueTypeArray
        {
            public byte[] ByteArray { get; set; }

            public HasValueTypeArray()
            {
                ByteArray = new byte[] {1, 2, 3};
            }
        }

        [Fact]
        public void SetterInjectionIgnoresArraysOfValueTypes()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<HasValueTypeArray>().PropertiesAutowired();
            var container = builder.Build();

            var instance = container.Resolve<HasValueTypeArray>();

            Assert.Equal(new byte[] {1, 2, 3}, instance.ByteArray);
        }

        public class HasNullableValueTypeArray
        {
            public double?[] DoubleArray { get; set; }

            public HasNullableValueTypeArray()
            {
                DoubleArray = new double?[] {null, 0.1, null};
            }
        }

        [Fact]
        public void SetterInjectionIgnoresArraysOfNullableValueTypes()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<HasNullableValueTypeArray>().PropertiesAutowired();
            var container = builder.Build();

            var instance = container.Resolve<HasNullableValueTypeArray>();

            Assert.Equal(new double?[] {null, 0.1, null}, instance.DoubleArray);
        }

        public class HasValueTypeList
        {
            public IList<byte> ByteListInterface { get; set; }

            public List<byte> ByteList { get; set; }

            public HasValueTypeList()
            {
                ByteList = new List<byte> {1, 2, 3};
                ByteListInterface = ByteList;
            }
        }

        [Fact]
        public void SetterInjectionIgnoresListsOfValueTypes()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<HasValueTypeList>().PropertiesAutowired();
            var container = builder.Build();

            var instance = container.Resolve<HasValueTypeList>();

            var expected = new List<byte> {1, 2, 3};
            Assert.Equal(expected, instance.ByteListInterface);
            Assert.Equal(expected, instance.ByteList);
        }

        public class HasNullableValueTypeList
        {
            public IList<double?> DoubleListInterface { get; set; }

            public List<double?> DoubleList { get; set; }

            public HasNullableValueTypeList()
            {
                DoubleList = new List<double?> {null, 0.1, null};
                DoubleListInterface = DoubleList;
            }
        }

        [Fact]
        public void SetterInjectionIgnoresListsOfNullableValueTypes()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<HasNullableValueTypeList>().PropertiesAutowired();
            var container = builder.Build();

            var instance = container.Resolve<HasNullableValueTypeList>();

            var expected = new List<double?> {null, 0.1, null};
            Assert.Equal(expected, instance.DoubleListInterface);
            Assert.Equal(expected, instance.DoubleList);
        }

        public class HasValueTypeCollection
        {
            public ICollection<byte> ByteCollectionInterface { get; set; }

            public Collection<byte> ByteCollection { get; set; }

            public HasValueTypeCollection()
            {
                ByteCollection = new Collection<byte> {1, 2, 3};
                ByteCollectionInterface = ByteCollection;
            }
        }

        [Fact]
        public void SetterInjectionIgnoresCollectionsOfValueTypes()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<HasValueTypeCollection>().PropertiesAutowired();
            var container = builder.Build();

            var instance = container.Resolve<HasValueTypeCollection>();

            var expected = new Collection<byte> {1, 2, 3};
            Assert.Equal(expected, instance.ByteCollectionInterface);
            Assert.Equal(expected, instance.ByteCollection);
        }

        public class HasNullableValueTypeCollection
        {
            public IReadOnlyCollection<double?> DoubleCollectionInterface { get; set; }

            public ReadOnlyCollection<double?> DoubleCollection { get; set; }

            public HasNullableValueTypeCollection()
            {
                DoubleCollection = new ReadOnlyCollection<double?>(new double?[] {null, 0.1, null});
                DoubleCollectionInterface = DoubleCollection;
            }
        }

        [Fact]
        public void SetterInjectionIgnoresCollectionsOfNullableValueTypes()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<HasNullableValueTypeCollection>().PropertiesAutowired();
            var container = builder.Build();

            var instance = container.Resolve<HasNullableValueTypeCollection>();

            var expected = new ReadOnlyCollection<double?>(new double?[] {null, 0.1, null});
            Assert.Equal(expected, instance.DoubleCollectionInterface);
            Assert.Equal(expected, instance.DoubleCollection);
        }
    }
}
