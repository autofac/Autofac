﻿// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Linq;
using System.Reflection;
using Autofac.Util;

namespace Autofac.Core.Activators.Reflection
{
    class AutowiringPropertyInjector
    {
        public const string InstanceTypeNamedParameter = "Autofac.AutowiringPropertyInjector.InstanceType";

        public static void InjectProperties(IComponentContext context, object instance, bool overrideSetValues)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            var instanceType = instance.GetType();

            foreach (var property in instanceType
                .GetTypeInfo().DeclaredProperties
                .Where(pi => pi.CanWrite))
            {
                var propertyType = property.PropertyType;

                if (propertyType.GetTypeInfo().IsValueType && !propertyType.GetTypeInfo().IsEnum)
                    continue;

                if (propertyType.IsArray && propertyType.GetElementType().GetTypeInfo().IsValueType)
                    continue;

                if (propertyType.IsGenericEnumerableInterfaceType() && propertyType.GetTypeInfo().GenericTypeArguments.ToArray()[0].GetTypeInfo().IsValueType)
                    continue;

                if (property.GetIndexParameters().Length != 0)
                    continue;

                if (!context.IsRegistered(propertyType))
                    continue;

                if (!property.SetMethod.IsPublic)
                    continue;

                if (!overrideSetValues &&
                    property.CanRead && property.CanWrite &&
                    (property.GetValue(instance, null) != null))
                    continue;

                var propertyValue = context.Resolve(propertyType, new NamedParameter(InstanceTypeNamedParameter, instanceType));
                property.SetValue(instance, propertyValue, null);
            }
        }
    }
}
