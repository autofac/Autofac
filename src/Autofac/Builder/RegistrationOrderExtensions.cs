// This software is part of the Autofac IoC container
// Copyright © 2016 Autofac Contributors
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

using Autofac.Core;

namespace Autofac.Builder
{
    internal static class RegistrationOrderExtensions
    {
        internal static long GetRegistrationOrder(this IComponentRegistration registration)
        {
            object value;
            return registration.Metadata.TryGetValue(MetadataKeys.RegistrationOrderMetadataKey, out value) ? (long)value : long.MaxValue;
        }

        internal static IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> InheritRegistrationOrderFrom<TLimit, TActivatorData, TSingleRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TSingleRegistrationStyle> registration,
                IComponentRegistration source)
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            var sourceRegistrationOrder = source.GetRegistrationOrder();
            registration.RegistrationData.Metadata[MetadataKeys.RegistrationOrderMetadataKey] = sourceRegistrationOrder;

            return registration;
        }
    }
}
