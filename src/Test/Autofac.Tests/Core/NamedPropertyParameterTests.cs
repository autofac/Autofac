using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Reflection;
using Autofac.Core;

namespace Autofac.Tests.Core
{
    [TestFixture]
    public class NamedPropertyParameterTests
    {
        class HasInjectionPoints
        {
            public const string PropertyName = "PropertyInjectionPoint";
            public const string WrongPropertyName = "WrongPropertyInjectionPoint";
            public const string MethodName = "MethodInjectionPoint";

            public HasInjectionPoints(string PropertyInjectionPoint) { }

            public void MethodInjectionPoint(string PropertyInjectionPoint) { }

            public string PropertyInjectionPoint { set { } }

            public string WrongPropertyInjectionPoint { set { } }
        }

        ParameterInfo GetSetAccessorParameter(PropertyInfo pi)
        {
            return pi
                .GetAccessors()
                .First()
                .GetParameters()
                .First();
        }

        ParameterInfo PropertySetValueParameter()
        {
            return GetSetAccessorParameter(
                    typeof(HasInjectionPoints)
                    .GetProperty(HasInjectionPoints.PropertyName));
                
        }

        ParameterInfo WrongPropertySetValueParameter()
        {
            return GetSetAccessorParameter(
                    typeof(HasInjectionPoints)
                    .GetProperty(HasInjectionPoints.WrongPropertyName));
        }

        ParameterInfo ConstructorParameter()
        {
            return typeof(HasInjectionPoints)
                .GetConstructors()
                .First()
                .GetParameters()
                .First();
        }

        ParameterInfo MethodParameter()
        {
            return typeof(HasInjectionPoints)
                .GetMethod(HasInjectionPoints.MethodName)
                .GetParameters()
                .First();
        }

        [Test]
        public void MatchesPropertySetterByName()
        {
            var cp = new NamedPropertyParameter(HasInjectionPoints.PropertyName, "");
            Func<object> vp;
            Assert.IsTrue(cp.CanSupplyValue(PropertySetValueParameter(), Container.Empty, out vp));
        }

        [Test]
        public void DoesNotMatchePropertySetterWithDifferentName()
        {
            var cp = new NamedPropertyParameter(HasInjectionPoints.PropertyName, "");
            Func<object> vp;
            Assert.IsFalse(cp.CanSupplyValue(WrongPropertySetValueParameter(), Container.Empty, out vp));
        }

        [Test]
        public void DoesNotMatchConstructorParameters()
        {
            var cp = new NamedPropertyParameter(HasInjectionPoints.PropertyName, "");
            Func<object> vp;
            Assert.IsFalse(cp.CanSupplyValue(ConstructorParameter(), Container.Empty, out vp));
        }

        [Test]
        public void DoesNotMatchRegularMethodParameters()
        {
            var cp = new NamedPropertyParameter(HasInjectionPoints.PropertyName, "");
            Func<object> vp;
            Assert.IsFalse(cp.CanSupplyValue(MethodParameter(), Container.Empty, out vp));
        }
    }
}
