using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;

namespace Autofac.Tests.Builder
{
    [TestFixture]
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

        [Test]
        public void SetterInjection()
        {
            var val = "Value";

            var builder = new ContainerBuilder();
            builder.RegisterInstance(val);
            builder.RegisterType<HasSetter>().PropertiesAutowired();

            var container = builder.Build();

            var instance = container.Resolve<HasSetter>();

            Assert.IsNotNull(instance);
            Assert.AreEqual(val, instance.Val);
        }

        [Test]
        public void SetterInjectionUnset()
        {
            var val = "Value";

            var builder = new ContainerBuilder();
            builder.RegisterInstance(val);
            builder.RegisterType<HasSetter>().PropertiesAutowired();

            var container = builder.Build();

            var instance = container.Resolve<HasSetter>();

            Assert.IsNotNull(instance);
            Assert.AreEqual(val, instance.Val);
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

        //[Test]
        //public void SetterInjectionUnsetWithValue()
        //{
        //    var val = "Value";

        //    var builder = new ContainerBuilder();
        //    builder.Register(val);
        //    builder.Register<HasSetterWithValue>()
        //        .OnActivating(ActivatingHandler.InjectUnsetProperties);

        //    var container = builder.Build();

        //    var instance = container.Resolve<HasSetterWithValue>();

        //    Assert.IsNotNull(instance);
        //    Assert.AreEqual("Default", instance.Val);
        //}

        [Test]
        public void SetterInjectionWithValue()
        {
            var val = "Value";

            var builder = new ContainerBuilder();
            builder.RegisterInstance(val);
            builder.RegisterType<HasSetterWithValue>().PropertiesAutowired();

            var container = builder.Build();

            var instance = container.Resolve<HasSetterWithValue>();

            Assert.IsNotNull(instance);
            Assert.AreEqual(val, instance.Val);
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

        [Test]
        public void SetterInjectionReadOnly()
        {
            var val = "Value";

            var builder = new ContainerBuilder();
            builder.RegisterInstance(val);
            builder.RegisterType<HasPropReadOnly>().PropertiesAutowired();

            var container = builder.Build();

            var instance = container.Resolve<HasPropReadOnly>();

            Assert.IsNotNull(instance);
            Assert.AreEqual("Default", instance.Val);
        }

        [Test]
        public void SetterInjectionUnsetReadOnly()
        {
            var val = "Value";

            var builder = new ContainerBuilder();
            builder.RegisterInstance(val);
            builder.RegisterType<HasPropReadOnly>().PropertiesAutowired();

            var container = builder.Build();

            var instance = container.Resolve<HasPropReadOnly>();

            Assert.IsNotNull(instance);
            Assert.AreEqual("Default", instance.Val);
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

        [Test]
        public void SetterInjectionWriteOnly()
        {
            var val = "Value";

            var builder = new ContainerBuilder();
            builder.RegisterInstance(val);
            builder.RegisterType<HasPropWriteOnly>().PropertiesAutowired();

            var container = builder.Build();
            var instance = container.Resolve<HasPropWriteOnly>();

            Assert.IsNotNull(instance);
            Assert.AreEqual(val, instance.GetVal());
        }

        [Test]
        public void SetterInjectionUnsetWriteOnly()
        {
            var val = "Value";

            var builder = new ContainerBuilder();
            builder.RegisterInstance(val);
            builder.RegisterType<HasPropWriteOnly>().PropertiesAutowired();

            var container = builder.Build();
            var instance = container.Resolve<HasPropWriteOnly>();

            Assert.IsNotNull(instance);
            Assert.AreEqual(val, instance.GetVal());
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

        [Test]
        public void SetterInjectionPrivateGet()
        {
            var val = "Value";

            var builder = new ContainerBuilder();
            builder.RegisterInstance(val);
            builder.RegisterType<SplitAccess>().PropertiesAutowired();

            var container = builder.Build();
            var instance = container.Resolve<SplitAccess>();

            Assert.IsNotNull(instance);
            Assert.IsTrue(instance.SetterCalled);
            Assert.IsFalse(instance.GetterCalled);
        }

        public class Invokee
        {
            public int Param { get; set; }

            public void Method(int param)
            {
                Param = param;
            }
        }

        [Test]
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
            Assert.AreEqual(pval, invokee.Param);
        }

        public class HasValueTypeArray
        {
            public byte[] ByteArray { get; set; }

            public HasValueTypeArray()
            {
                ByteArray = new byte[] {1, 2, 3};
            }
        }

        [Test]
        public void SetterInjectionIgnoresArraysOfValueTypes()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<HasValueTypeArray>().PropertiesAutowired();
            var container = builder.Build();

            var instance = container.Resolve<HasValueTypeArray>();

            Assert.That(instance.ByteArray, Is.EqualTo(new byte[] {1, 2, 3}));
        }

        public class HasNullableValueTypeArray
        {
            public double?[] DoubleArray { get; set; }

            public HasNullableValueTypeArray()
            {
                DoubleArray = new double?[] {null, 0.1, null};
            }
        }

        [Test]
        public void SetterInjectionIgnoresArraysOfNullableValueTypes()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<HasNullableValueTypeArray>().PropertiesAutowired();
            var container = builder.Build();

            var instance = container.Resolve<HasNullableValueTypeArray>();

            Assert.That(instance.DoubleArray, Is.EqualTo(new double?[] {null, 0.1, null}));
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

        [Test]
        public void SetterInjectionIgnoresListsOfValueTypes()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<HasValueTypeList>().PropertiesAutowired();
            var container = builder.Build();

            var instance = container.Resolve<HasValueTypeList>();

            var expected = new List<byte> {1, 2, 3};
            Assert.That(instance.ByteListInterface, Is.EqualTo(expected));
            Assert.That(instance.ByteList, Is.EqualTo(expected));
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

        [Test]
        public void SetterInjectionIgnoresListsOfNullableValueTypes()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<HasNullableValueTypeList>().PropertiesAutowired();
            var container = builder.Build();

            var instance = container.Resolve<HasNullableValueTypeList>();

            var expected = new List<double?> {null, 0.1, null};
            Assert.That(instance.DoubleListInterface, Is.EqualTo(expected));
            Assert.That(instance.DoubleList, Is.EqualTo(expected));
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

        [Test]
        public void SetterInjectionIgnoresCollectionsOfValueTypes()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<HasValueTypeCollection>().PropertiesAutowired();
            var container = builder.Build();

            var instance = container.Resolve<HasValueTypeCollection>();

            var expected = new Collection<byte> {1, 2, 3};
            Assert.That(instance.ByteCollectionInterface, Is.EqualTo(expected));
            Assert.That(instance.ByteCollection, Is.EqualTo(expected));
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

        [Test]
        public void SetterInjectionIgnoresCollectionsOfNullableValueTypes()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<HasNullableValueTypeCollection>().PropertiesAutowired();
            var container = builder.Build();

            var instance = container.Resolve<HasNullableValueTypeCollection>();

            var expected = new ReadOnlyCollection<double?>(new double?[] {null, 0.1, null});
            Assert.That(instance.DoubleCollectionInterface, Is.EqualTo(expected));
            Assert.That(instance.DoubleCollection, Is.EqualTo(expected));
        }
    }
}
