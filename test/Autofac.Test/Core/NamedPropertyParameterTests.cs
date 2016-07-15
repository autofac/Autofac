using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Autofac.Core;
using Xunit;

namespace Autofac.Test.Core
{
    public class NamedPropertyParameterTests
    {
        public class HasInjectionPoints
        {
            public const string PropertyName = "PropertyInjectionPoint";

            public const string WrongPropertyName = "WrongPropertyInjectionPoint";

            public const string MethodName = "MethodInjectionPoint";

            public HasInjectionPoints(string propertyInjectionPoint)
            {
            }

            public void MethodInjectionPoint(string propertyInjectionPoint)
            {
            }

            public string PropertyInjectionPoint
            {
                set
                {
                }
            }

            public string WrongPropertyInjectionPoint
            {
                set
                {
                }
            }
        }

        private ParameterInfo GetSetAccessorParameter(PropertyInfo pi)
        {
            return pi
                .GetAccessors()
                .First()
                .GetParameters()
                .First();
        }

        private ParameterInfo PropertySetValueParameter()
        {
            return GetSetAccessorParameter(
                    typeof(HasInjectionPoints)
                    .GetProperty(HasInjectionPoints.PropertyName));
        }

        private ParameterInfo WrongPropertySetValueParameter()
        {
            return GetSetAccessorParameter(
                    typeof(HasInjectionPoints)
                    .GetProperty(HasInjectionPoints.WrongPropertyName));
        }

        private ParameterInfo ConstructorParameter()
        {
            return typeof(HasInjectionPoints)
                .GetConstructors()
                .First()
                .GetParameters()
                .First();
        }

        private ParameterInfo MethodParameter()
        {
            return typeof(HasInjectionPoints)
                .GetMethod(HasInjectionPoints.MethodName)
                .GetParameters()
                .First();
        }

        [Fact]
        public void MatchesPropertySetterByName()
        {
            var cp = new NamedPropertyParameter(HasInjectionPoints.PropertyName, "");
            Func<object> vp;
            Assert.True(cp.CanSupplyValue(PropertySetValueParameter(), new ContainerBuilder().Build(), out vp));
        }

        [Fact]
        public void DoesNotMatchePropertySetterWithDifferentName()
        {
            var cp = new NamedPropertyParameter(HasInjectionPoints.PropertyName, "");
            Func<object> vp;
            Assert.False(cp.CanSupplyValue(WrongPropertySetValueParameter(), new ContainerBuilder().Build(), out vp));
        }

        [Fact]
        public void DoesNotMatchConstructorParameters()
        {
            var cp = new NamedPropertyParameter(HasInjectionPoints.PropertyName, "");
            Func<object> vp;
            Assert.False(cp.CanSupplyValue(ConstructorParameter(), new ContainerBuilder().Build(), out vp));
        }

        [Fact]
        public void DoesNotMatchRegularMethodParameters()
        {
            var cp = new NamedPropertyParameter(HasInjectionPoints.PropertyName, "");
            Func<object> vp;
            Assert.False(cp.CanSupplyValue(MethodParameter(), new ContainerBuilder().Build(), out vp));
        }
    }
}
