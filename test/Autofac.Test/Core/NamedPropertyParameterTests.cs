// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Core;

namespace Autofac.Test.Core;

public class NamedPropertyParameterTests
{
    private class HasInjectionPoints
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

        [SuppressMessage("CA1044", "CA1044", Justification = "Test scenario.")]
        public string PropertyInjectionPoint
        {
            set
            {
            }
        }

        [SuppressMessage("CA1044", "CA1044", Justification = "Test scenario.")]
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
        Assert.True(cp.CanSupplyValue(PropertySetValueParameter(), new ContainerBuilder().Build(), out Func<object> vp));
    }

    [Fact]
    public void DoesNotMatchePropertySetterWithDifferentName()
    {
        var cp = new NamedPropertyParameter(HasInjectionPoints.PropertyName, "");
        Assert.False(cp.CanSupplyValue(WrongPropertySetValueParameter(), new ContainerBuilder().Build(), out Func<object> vp));
    }

    [Fact]
    public void DoesNotMatchConstructorParameters()
    {
        var cp = new NamedPropertyParameter(HasInjectionPoints.PropertyName, "");
        Assert.False(cp.CanSupplyValue(ConstructorParameter(), new ContainerBuilder().Build(), out Func<object> vp));
    }

    [Fact]
    public void DoesNotMatchRegularMethodParameters()
    {
        var cp = new NamedPropertyParameter(HasInjectionPoints.PropertyName, "");
        Assert.False(cp.CanSupplyValue(MethodParameter(), new ContainerBuilder().Build(), out Func<object> vp));
    }
}
