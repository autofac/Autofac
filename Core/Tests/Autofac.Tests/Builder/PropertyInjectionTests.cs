using Autofac.Builder;
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
    }
}
