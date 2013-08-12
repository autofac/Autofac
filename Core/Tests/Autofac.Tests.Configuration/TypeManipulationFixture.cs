using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using Autofac.Configuration;
using Autofac.Configuration.Elements;
using Autofac.Configuration.Util;
using NUnit.Framework;

namespace Autofac.Tests.Configuration
{
    [TestFixture]
    public class TypeManipulationFixture
    {
        [Test]
        public void ChangeToCompatibleTypeLooksForTryParseMethod()
        {
            const string address = "127.0.0.1";
            var value = TypeManipulation.ChangeToCompatibleType(address, typeof(IPAddress), null);
            Assert.That(value, Is.EqualTo(IPAddress.Parse(address)));
        }

        [Test]
        public void DictionaryConversionUsesTypeConverterAttribute()
        {
            var container = ConfigureContainer();
            var obj = container.Resolve<HasDictionaryProperty>();
            Assert.IsNotNull(obj.Dictionary);
            Assert.AreEqual(2, obj.Dictionary.Count);
            Assert.AreEqual(1, obj.Dictionary["a"].Value);
            Assert.AreEqual(2, obj.Dictionary["b"].Value);
        }

        [Test]
        public void ListConversionUsesTypeConverterAttribute()
        {
            var container = ConfigureContainer();
            var obj = container.Resolve<HasEnumerableProperty>();
            Assert.IsNotNull(obj.List);
            Assert.AreEqual(2, obj.List.Count);
            Assert.AreEqual(1, obj.List[0].Value);
            Assert.AreEqual(2, obj.List[1].Value);
        }

        [Test]
        public void ParameterConversionUsesTypeConverterAttribute()
        {
            var container = ConfigureContainer();
            var obj = container.Resolve<HasParametersAndProperties>();
            Assert.IsNotNull(obj.Parameter);
            Assert.AreEqual(1, obj.Parameter.Value);
        }

        [Test]
        public void PropertyConversionUsesTypeConverterAttribute()
        {
            var container = ConfigureContainer();
            var obj = container.Resolve<HasParametersAndProperties>();
            Assert.IsNotNull(obj.Property);
            Assert.AreEqual(2, obj.Property.Value);
        }

        public class Convertible
        {
            public int Value { get; set; }
        }

        public class ConvertibleConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value == null)
                {
                    return null;
                }
                var str = value as String;
                if (str == null)
                {
                    return base.ConvertFrom(context, culture, value);
                }
                var converter = TypeDescriptor.GetConverter(typeof(int));
                return new Convertible { Value = (int)converter.ConvertFromString(context, culture, str) };
            }
        }

        public class ConvertibleListConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(ListElementCollection) || base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value == null)
                {
                    return null;
                }
                var castValue = value as ListElementCollection;
                if (castValue == null)
                {
                    return base.ConvertFrom(context, culture, value);
                }
                var list = new List<Convertible>();
                var converter = new ConvertibleConverter();
                foreach (var item in castValue)
                {
                    list.Add((Convertible)converter.ConvertFrom(item.Value));
                }
                return list;
            }
        }

        public class ConvertibleDictionaryConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(DictionaryElementCollection) || base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value == null)
                {
                    return null;
                }
                var castValue = value as DictionaryElementCollection;
                if (castValue == null)
                {
                    return base.ConvertFrom(context, culture, value);
                }
                var dict = new Dictionary<string, Convertible>();
                var converter = new ConvertibleConverter();
                foreach (var item in castValue)
                {
                    dict[item.Key] = (Convertible)converter.ConvertFrom(item.Value);
                }
                return dict;
            }
        }

        public class HasDictionaryProperty
        {
            [TypeConverter(typeof(ConvertibleDictionaryConverter))]
            public IDictionary<string, Convertible> Dictionary { get; set; }
        }

        public class HasEnumerableProperty
        {
            [TypeConverter(typeof(ConvertibleListConverter))]
            public IList<Convertible> List { get; set; }
        }

        public class HasParametersAndProperties
        {
            public HasParametersAndProperties([TypeConverter(typeof(ConvertibleConverter))] Convertible parameter) { Parameter = parameter; }

            public Convertible Parameter { get; set; }

            [TypeConverter(typeof(ConvertibleConverter))]
            public Convertible Property { get; set; }
        }

        private static IContainer ConfigureContainer()
        {
            var cb = new ContainerBuilder();
            var csr = new ConfigurationSettingsReader(SectionHandler.DefaultSectionName, "Files/TypeManipulation.config");
            cb.RegisterModule(csr);
            return cb.Build();
        }
    }
}