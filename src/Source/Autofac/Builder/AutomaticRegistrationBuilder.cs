// This software is part of the Autofac IoC container
// Copyright (c) 2007 Nicholas Blumhardt
// nicholas.blumhardt@gmail.com
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
using System.Reflection;
using Autofac.Registrars;
using Autofac.Registrars.Automatic;

namespace Autofac.Builder
{
    /// <summary>
    /// 
    /// </summary>
    public static class AutomaticRegistrationBuilder
    {
        /// <summary>
        /// Registers the types matching.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        public static IGenericRegistrar RegisterTypesMatching(
            this ContainerBuilder builder,
            Predicate<Type> predicate)
        {
            Enforce.ArgumentNotNull(builder, "builder");
            Enforce.ArgumentNotNull(predicate, "predicate");
            var module = new AutomaticRegistrar(predicate);
            builder.RegisterModule(module);
            return module;
        }

        /// <summary>
        /// Registers the types from assembly.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        public static IGenericRegistrar RegisterTypesFromAssembly(
            this ContainerBuilder builder,
            Assembly assembly)
        {
            Enforce.ArgumentNotNull(builder, "builder");
            Enforce.ArgumentNotNull(assembly, "assembly");
            return RegisterTypesMatching(builder, t => t.Assembly == assembly);
        }

        /// <summary>
        /// Registers the types assignable to.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IGenericRegistrar RegisterTypesAssignableTo<T>(
            this ContainerBuilder builder)
        {
            Enforce.ArgumentNotNull(builder, "builder");
            return RegisterTypesMatching(builder, t => typeof(T).IsAssignableFrom(t));
        }
    }
}
