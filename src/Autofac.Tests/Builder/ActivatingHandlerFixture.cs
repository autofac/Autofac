using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Autofac.Builder;

namespace Autofac.Tests.Builder
{
    [TestFixture]
    public class ActivatingHandlerFixture
    {
        class HasSetter
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
            builder.Register(val);
            builder.Register<HasSetter>()
                .OnActivating(ActivatingHandler.InjectProperties);

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
            builder.Register(val);
            builder.Register<HasSetter>()
                .OnActivating(ActivatingHandler.InjectUnsetProperties);

            var container = builder.Build();

            var instance = container.Resolve<HasSetter>();

            Assert.IsNotNull(instance);
            Assert.AreEqual(val, instance.Val);
        }

        class HasSetterWithValue
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

        [Test]
        public void SetterInjectionUnsetWithValue()
        {
            var val = "Value";

            var builder = new ContainerBuilder();
            builder.Register(val);
            builder.Register<HasSetterWithValue>()
                .OnActivating(ActivatingHandler.InjectUnsetProperties);

            var container = builder.Build();

            var instance = container.Resolve<HasSetterWithValue>();

            Assert.IsNotNull(instance);
            Assert.AreEqual("Default", instance.Val);
        }

        [Test]
        public void SetterInjectionWithValue()
        {
            var val = "Value";

            var builder = new ContainerBuilder();
            builder.Register(val);
            builder.Register<HasSetterWithValue>()
                .OnActivating(ActivatingHandler.InjectProperties);

            var container = builder.Build();

            var instance = container.Resolve<HasSetterWithValue>();

            Assert.IsNotNull(instance);
            Assert.AreEqual(val, instance.Val);
        }

        class HasPropReadOnly
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
            builder.Register(val);
            builder.Register<HasPropReadOnly>()
                .OnActivating(ActivatingHandler.InjectProperties);

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
            builder.Register(val);
            builder.Register<HasPropReadOnly>()
                .OnActivating(ActivatingHandler.InjectUnsetProperties);

            var container = builder.Build();

            var instance = container.Resolve<HasPropReadOnly>();

            Assert.IsNotNull(instance);
            Assert.AreEqual("Default", instance.Val);
        }

        class HasPropWriteOnly
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
            builder.Register(val);
            builder.Register<HasPropWriteOnly>()
                .OnActivating(ActivatingHandler.InjectProperties);

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
            builder.Register(val);
            builder.Register<HasPropWriteOnly>()
                .OnActivating(ActivatingHandler.InjectUnsetProperties);

            var container = builder.Build();
            var instance = container.Resolve<HasPropWriteOnly>();

            Assert.IsNotNull(instance);
            Assert.AreEqual(val, instance.GetVal());
        }

        class SplitAccess
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
            builder.Register(val);
            builder.Register<SplitAccess>()
                .OnActivating(ActivatingHandler.InjectUnsetProperties);

            var container = builder.Build();
            var instance = container.Resolve<SplitAccess>();

            Assert.IsNotNull(instance);
            Assert.IsTrue(instance.SetterCalled);
            Assert.IsFalse(instance.GetterCalled);
        }
    }
}
