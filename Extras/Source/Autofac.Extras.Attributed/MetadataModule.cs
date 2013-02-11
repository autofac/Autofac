// This software is part of the Autofac IoC container
// Copyright © 2013 Autofac Contributors
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
using Autofac.Builder;
using Autofac.Integration.Mef;

namespace Autofac.Extras.Attributed
{
    /// <summary>
    /// Provides a mechanism to separate metadata registrations from compile-time attributes
    /// </summary>
    /// <typeparam name="TInterface">interface used on concrete types of metadata decorated instances</typeparam>
    /// <typeparam name="TMetadata">strongly typed metadata definition</typeparam>
    public interface IMetadataRegistrar<in TInterface, in TMetadata>
    {
        /// <summary>
        /// registers provided metadata on the declared type 
        /// </summary>
        /// <typeparam name="TInstance">concrete instance type</typeparam>
        /// <param name="metadata">metadata instance</param>
        /// <returns>container builder</returns>
        IRegistrationBuilder<TInstance, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterType
            <TInstance>(TMetadata metadata) where TInstance : TInterface;

        /// <summary>
        /// registers provided metadata on the declared type
        /// </summary>
        /// <param name="instanceType">Type of the instance.</param>
        /// <param name="metadata">The metadata.</param>
        /// <returns>registration builder</returns>
        IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterType
            (Type instanceType, TMetadata metadata);

        /// <summary>
        /// registers the provided concrete instance and scans it for generic MetadataAttribute data
        /// </summary>
        /// <typeparam name="TInstance">concrete instance type</typeparam>
        /// <returns></returns>
        IRegistrationBuilder<TInstance, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterAttributedType
            <TInstance>() where TInstance : TInterface;

        /// <summary>
        /// registers the provided concrrete instance type and scans it for generate metadata data
        /// </summary>
        /// <param name="instanceType">Type of the instance.</param>
        /// <returns></returns>
        IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterAttributedType
            (Type instanceType);

        /// <summary>
        /// Used to build an <see cref="IContainer"/> from component registrations.
        /// </summary>
        ContainerBuilder ContainerBuilder { get; }
    }


    /// <summary>
    /// Provides a mechanism to separate metadata registrations from compile-time attributes
    /// </summary>
    /// <typeparam name="TInterface">interface used on concrete types of metadata decorated instances</typeparam>
    /// <typeparam name="TMetadata">strongly typed metadata definition</typeparam>
    public abstract class MetadataModule<TInterface, TMetadata> : Module, IMetadataRegistrar<TInterface, TMetadata>
    {
        /// <summary>
        /// client overrided method where metadata registration is performed
        /// </summary>
        /// <param name="registrar">wrapped metadata registry interface</param>
        public abstract void Register(IMetadataRegistrar<TInterface, TMetadata> registrar);

        /// <summary>
        /// standard module method being overloaded and sealed to provide wrapped metadata registration
        /// </summary>
        /// <param name="builder">container builder</param>
        sealed protected override void Load(ContainerBuilder builder)
        {
            ContainerBuilder = builder;
            builder.RegisterMetadataRegistrationSources();
            Register(this);
        }

        #region IMetadataRegistrar<TInterface,TMetadata> Members

        /// <summary>
        /// registers provided metadata on the declared type 
        /// </summary>
        /// <typeparam name="TInstance">concrete instance type</typeparam>
        /// <param name="metadata">metadata instance</param>
        /// <returns>container builder</returns>
        public IRegistrationBuilder<TInstance, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterType<TInstance>(TMetadata metadata) where TInstance : TInterface
        {
            return
                ContainerBuilder.RegisterType<TInstance>().As<TInterface>().WithMetadata(
                    MetadataHelper.GetProperties(metadata));
        }

        /// <summary>
        /// registers the provided concrete instance and scans it for generic MetadataAttribute data
        /// </summary>
        /// <typeparam name="TInstance">concrete instance type</typeparam>
        /// <returns></returns>
        public IRegistrationBuilder<TInstance, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterAttributedType<TInstance>() where TInstance : TInterface
        {
            return
                ContainerBuilder.RegisterType<TInstance>().As<TInterface>().WithMetadata(
                    MetadataHelper.GetMetadata(typeof(TInstance)));
        }

        /// <summary>
        /// registers provided metadata on the declared type
        /// </summary>
        /// <param name="instanceType">Type of the instance.</param>
        /// <param name="metadata">The metadata.</param>
        /// <returns>registration builder</returns>
        public IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterType(Type instanceType, TMetadata metadata)
        {
            return
                ContainerBuilder.RegisterType(instanceType).As<TInterface>().WithMetadata(
                    MetadataHelper.GetProperties(metadata));
        }

        /// <summary>
        /// registers the provided concrrete instance type and scans it for generate metadata data
        /// </summary>
        /// <param name="instanceType">Type of the instance.</param>
        /// <returns></returns>
        public IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterAttributedType(Type instanceType)
        {
            return
                ContainerBuilder.RegisterType(instanceType).As<TInterface>().WithMetadata(
                    MetadataHelper.GetMetadata(instanceType));
        }

        /// <summary>
        /// Used to build an <see cref="IContainer"/> from component registrations.
        /// </summary>
        public ContainerBuilder ContainerBuilder { get; private set; }

        #endregion
    }
}
