using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Xunit;

namespace Autofac.Test.Builder
{
    public class PropertyInjectionTests
    {
        public class HasSetter
        {
            private string _val;

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
            private string _val = "Default";

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
            private string _val = "Default";

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
            private string _val;

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
                ByteArray = new byte[] { 1, 2, 3 };
            }
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

        public class HasNullableValueTypeArray
        {
            public double?[] DoubleArray { get; set; }

            public HasNullableValueTypeArray()
            {
                DoubleArray = new double?[] { null, 0.1, null };
            }
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

        public class HasValueTypeList
        {
            public IList<byte> ByteListInterface { get; set; }

            public List<byte> ByteList { get; set; }

            public HasValueTypeList()
            {
                ByteList = new List<byte> { 1, 2, 3 };
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

            var expected = new List<byte> { 1, 2, 3 };
            Assert.Equal(expected, instance.ByteListInterface);
            Assert.Equal(expected, instance.ByteList);
        }

        public class HasNullableValueTypeList
        {
            public IList<double?> DoubleListInterface { get; set; }

            public List<double?> DoubleList { get; set; }

            public HasNullableValueTypeList()
            {
                DoubleList = new List<double?> { null, 0.1, null };
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

            var expected = new List<double?> { null, 0.1, null };
            Assert.Equal(expected, instance.DoubleListInterface);
            Assert.Equal(expected, instance.DoubleList);
        }

        public class HasValueTypeCollection
        {
            public ICollection<byte> ByteCollectionInterface { get; set; }

            public Collection<byte> ByteCollection { get; set; }

            public HasValueTypeCollection()
            {
                ByteCollection = new Collection<byte> { 1, 2, 3 };
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

            var expected = new Collection<byte> { 1, 2, 3 };
            Assert.Equal(expected, instance.ByteCollectionInterface);
            Assert.Equal(expected, instance.ByteCollection);
        }

        public class HasNullableValueTypeCollection
        {
            public IReadOnlyCollection<double?> DoubleCollectionInterface { get; set; }

            public ReadOnlyCollection<double?> DoubleCollection { get; set; }

            public HasNullableValueTypeCollection()
            {
                DoubleCollection = new ReadOnlyCollection<double?>(new double?[] { null, 0.1, null });
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

            var expected = new ReadOnlyCollection<double?>(new double?[] { null, 0.1, null });
            Assert.Equal(expected, instance.DoubleCollectionInterface);
            Assert.Equal(expected, instance.DoubleCollection);
        }

        [Fact]
        public void InjectProperties()
        {
            const string str = "test";

            var cb = new ContainerBuilder();
            cb.RegisterInstance(str);
            var c = cb.Build();

            var obj = new WithPropInjection();

            Assert.Null(obj.Prop);
            c.InjectUnsetProperties(obj);
            Assert.Equal(str, obj.Prop);
            Assert.Null(obj.GetProp2());
        }

        [Fact]
        public void InjectUnsetProperties()
        {
            const string str = "test";
            const string otherStr = "someString";

            var cb = new ContainerBuilder();
            cb.RegisterInstance(str);
            var c = cb.Build();

            var obj = new WithPropInjection
            {
                Prop = otherStr,
            };

            Assert.Equal(otherStr, obj.Prop);
            c.InjectUnsetProperties(obj);
            Assert.Equal(otherStr, obj.Prop);
            Assert.Null(obj.GetProp2());
        }

        [Fact]
        public void InjectPropertiesWithSelector()
        {
            const string str = "test";

            var cb = new ContainerBuilder();
            cb.RegisterInstance(str);
            var c = cb.Build();

            var obj = new WithPropInjection();

            Assert.Null(obj.Prop);
            c.InjectProperties(obj, new DelegatePropertySelector((p, _) => p.GetCustomAttributes<InjectAttribute>().Any()));
            Assert.Null(obj.Prop);
            Assert.Equal(str, obj.GetProp2());
        }

        [Fact]
        public void ResolvePropertiesWithCustomDelegate_ReflectionRegistration()
        {
            var propertyInfos = new List<PropertyInfo>();

            var cb = new ContainerBuilder();
            cb.RegisterType<WithPropInjection>()
                .PropertiesAutowired((propInfo, instance) =>
                {
                    propertyInfos.Add(propInfo);
                    return false;
                });
            cb.RegisterInstance("test"); // Must register, otherwise delegate won't be called

            var c = cb.Build();
            var result = c.Resolve<WithPropInjection>();
            var expected = typeof(WithPropInjection).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.Equal(expected, propertyInfos.ToArray());
            Assert.NotNull(result);
            Assert.Null(result.Prop);
            Assert.Null(result.GetProp2());
        }

        [Fact]
        public void ResolvePropertiesWithCustomDelegate_DelegateRegistration()
        {
            var propertyInfos = new List<PropertyInfo>();

            var cb = new ContainerBuilder();
            cb.Register(_ => new WithPropInjection())
                .PropertiesAutowired((propInfo, instance) =>
                {
                    propertyInfos.Add(propInfo);
                    return false;
                });
            cb.RegisterInstance("test"); // Must register, otherwise delegate won't be called

            var c = cb.Build();
            var result = c.Resolve<WithPropInjection>();
            var expected = typeof(WithPropInjection).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.Equal(expected, propertyInfos.ToArray());
            Assert.NotNull(result);
            Assert.Null(result.Prop);
            Assert.Null(result.GetProp2());
        }

        [Fact]
        public void ResolvePropertiesWithCustomImplementation_ReflectionRegistration()
        {
            const string str = "test";

            var cb = new ContainerBuilder();
            cb.RegisterType<WithPropInjection>()
                .PropertiesAutowired(new InjectAttributePropertySelector());
            cb.RegisterInstance(str);

            var c = cb.Build();
            var result = c.Resolve<WithPropInjection>();

            Assert.NotNull(result);
            Assert.Null(result.Prop);
            Assert.Equal(str, result.GetProp2());
        }

        [Fact]
        public void ResolvePropertiesWithCustomImplementation_DelegateRegistration()
        {
            const string str = "test";

            var cb = new ContainerBuilder();
            cb.Register(_ => new WithPropInjection())
                .PropertiesAutowired(new InjectAttributePropertySelector());
            cb.RegisterInstance(str);

            var c = cb.Build();
            var result = c.Resolve<WithPropInjection>();

            Assert.NotNull(result);
            Assert.Null(result.Prop);
            Assert.Equal(str, result.GetProp2());
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

        private class InjectAttributePropertySelector : IPropertySelector
        {
            public bool InjectProperty(PropertyInfo propertyInfo, object instance)
            {
                return propertyInfo.GetCustomAttributes<InjectAttribute>().Any();
            }
        }

        private class InjectAttribute : Attribute
        {
        }

        private class WithPropInjection
        {
            public string Prop { get; set; }

            [Inject]
            private string Prop2 { get; set; }

            public string GetProp2() => Prop2;
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
            public ConstructorParamNotAttachedToProperty(string id)
            {
                this._id = id;
            }

            [SuppressMessage("SA1401", "SA1401")]
            public string _id = null;

            public string Name { get; set; }
        }
    }
}
