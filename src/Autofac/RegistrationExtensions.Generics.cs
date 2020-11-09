// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.OpenGenerics;

namespace Autofac
{
    /// <summary>
    /// Adds registration syntax to the <see cref="ContainerBuilder"/> type.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public static partial class RegistrationExtensions
    {
        /// <summary>
        /// Register an un-parameterised generic type, e.g. Repository&lt;&gt;.
        /// Concrete types will be made as they are requested, e.g. with Resolve&lt;Repository&lt;int&gt;&gt;().
        /// </summary>
        /// <param name="builder">Container builder.</param>
        /// <param name="implementer">The open generic implementation type.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>
            RegisterGeneric(this ContainerBuilder builder, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementer)
        {
            return OpenGenericRegistrationExtensions.RegisterGeneric(builder, implementer);
        }

        /// <summary>
        /// Register a delegate that can provide instances of an open generic registration.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="factory">Delegate responsible for generating an instance of a closed generic based on the open generic type being registered. It will be called with the current <see cref="IComponentContext"/> and the array of generic type arguments expected to be in the closed generic.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, OpenGenericDelegateActivatorData, DynamicRegistrationStyle>
            RegisterGeneric(this ContainerBuilder builder, Func<IComponentContext, Type[], object> factory)
        {
            return builder.RegisterGeneric((ctxt, types, parameters) => factory(ctxt, types));
        }

        /// <summary>
        /// Register a delegate that can provide instances of an open generic registration.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="factory">Delegate responsible for generating an instance of a closed generic based on the open generic type being registered. It will be called with the current <see cref="IComponentContext"/>, the array of generic type arguments expected to be in the closed generic, and any parameters passed to the resolve operation.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static IRegistrationBuilder<object, OpenGenericDelegateActivatorData, DynamicRegistrationStyle>
            RegisterGeneric(this ContainerBuilder builder, Func<IComponentContext, Type[], IEnumerable<Parameter>, object> factory)
        {
            return OpenGenericRegistrationExtensions.RegisterGeneric(builder, factory);
        }
    }
}
