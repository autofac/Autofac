// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2008 Autofac Contributors
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
using System.Collections.Generic;
using Autofac.Component.Activation;
using System.Linq;

namespace Autofac.Registrars.Generic
{
    /// <summary>
    /// Creates generic component registrations that will be automatically bound to concrete
    /// types as they are requested.
    /// </summary>
    /// <remarks>
    /// The interface of this class and returned IRegistrar are non-generic as
    /// C# (or the CLR) does not allow partially-constructed generic types like IList&lt;&gt;
    /// to be used as generic arguments.
    /// </remarks>
	public class GenericRegistrar : Registrar<IGenericRegistrar>, IModule, IGenericRegistrar
	{
		Type _implementor;
        IConstructorSelector _constructorSelector = new MostParametersConstructorSelector();
        IEnumerable<Parameter> _arguments = Enumerable.Empty<Parameter>();
        IEnumerable<NamedPropertyParameter> _properties = Enumerable.Empty<NamedPropertyParameter>();

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericRegistrar"/> class.
        /// </summary>
        /// <param name="implementor">The implementor.</param>
		public GenericRegistrar(Type implementor)
		{
            Enforce.ArgumentNotNull(implementor, "implementor");
			_implementor = implementor;
		}

		#region IModule Members

        /// <summary>
        /// Registers the component.
        /// </summary>
        /// <param name="container">The container.</param>
        protected override void DoConfigure(IContainer container)
        {
            Enforce.ArgumentNotNull(container, "container");
            
            var services = new List<Service>(Services);
            if (services.Count == 0)
                services.Add(new TypedService(_implementor));

			container.AddRegistrationSource(new GenericRegistrationHandler(
				services,
				_implementor,
                new DeferredRegistrationParameters(
                    Ownership,
                    Scope,
                    PreparingHandlers,
                    ActivatingHandlers,
                    ActivatedHandlers,
                    RegistrationCreator),
               _constructorSelector,
               _arguments,
               _properties));

            FireRegistered(new RegisteredEventArgs() { Container = container });
		}

		#endregion

        /// <summary>
        /// Returns this instance, correctly-typed.
        /// </summary>
        /// <value></value>
        protected override IGenericRegistrar Syntax
        {
            get { return this; }
        }

        /// <summary>
        /// Enforce that the specific constructor with the provided signature is used.
        /// </summary>
        /// <param name="ctorSignature">The types that designate the constructor to use.</param>
        /// <returns>A registrar allowing registration to continue.</returns>
        public virtual IGenericRegistrar UsingConstructor(params Type[] ctorSignature)
        {
            Enforce.ArgumentNotNull(ctorSignature, "ctorSignature");
            _constructorSelector = new SpecificConstructorSelector(ctorSignature);
            return this;
        }

        /// <summary>
        /// Associates constructor parameters with default values.
        /// </summary>
        /// <param name="additionalCtorArgs">The named values to apply to the constructor.
        /// These may be overriden by supplying any/all values to the IContext.Resolve() method.</param>
        /// <returns>
        /// A registrar allowing registration to continue.
        /// </returns>
        public virtual IGenericRegistrar WithArguments(params Parameter[] additionalCtorArgs)
        {
            return WithArguments((IEnumerable<Parameter>)additionalCtorArgs);
        }

        /// <summary>
        /// Associates constructor parameters with default values.
        /// </summary>
        /// <param name="additionalCtorArgs">The named values to apply to the constructor.
        /// These may be overriden by supplying any/all values to the IContext.Resolve() method.</param>
        /// <returns>
        /// A registrar allowing registration to continue.
        /// </returns>
        public virtual IGenericRegistrar WithArguments(IEnumerable<Parameter> additionalCtorArgs)
        {
            _arguments = _arguments.Union(additionalCtorArgs).ToArray();
            return Syntax;
        }

        /// <summary>
        /// Provide explicit property values to be set on the new object.
        /// </summary>
        /// <param name="explicitProperties"></param>
        /// <returns>A registrar allowing configuration to continue.</returns>
        /// <remarks>Note, supplying a null value will not prevent property injection if
        /// property injection is done through an OnActivating handler.</remarks>
        public virtual IGenericRegistrar WithProperties(params NamedPropertyParameter[] explicitProperties)
        {
            return WithProperties((IEnumerable<NamedPropertyParameter>)explicitProperties);
        }

        /// <summary>
        /// Provide explicit property values to be set on the new object.
        /// </summary>
        /// <param name="explicitProperties"></param>
        /// <returns>A registrar allowing configuration to continue.</returns>
        /// <remarks>Note, supplying a null value will not prevent property injection if
        /// property injection is done through an OnActivating handler.</remarks>
        public virtual IGenericRegistrar WithProperties(IEnumerable<NamedPropertyParameter> explicitProperties)
        {
            _properties = _properties.Union(explicitProperties).ToArray();
            return Syntax;
        }
    }
}
