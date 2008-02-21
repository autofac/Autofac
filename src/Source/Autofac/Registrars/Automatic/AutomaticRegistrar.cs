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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Component;
using Autofac.Builder;

namespace Autofac.Registrars.Automatic
{
    /// <summary>
    /// Automatically registers types that match a supplied predicate.
    /// </summary>
    class AutomaticRegistrar : Registrar<IGenericRegistrar>, IModule, IGenericRegistrar
    {
        Predicate<Type> _predicate;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomaticRegistrar"/> class.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        public AutomaticRegistrar(Predicate<Type> predicate)
        {
            Enforce.ArgumentNotNull(predicate, "predicate");
            _predicate = predicate;
        }

        #region IModule Members

        /// <summary>
        /// Registers the component.
        /// </summary>
        /// <param name="container">The container.</param>
        public void Configure(IContainer container)
        {
            Enforce.ArgumentNotNull(container, "container");
            container.AddRegistrationSource(new AutomaticRegistrationHandler(
                _predicate,
                Ownership,
                Scope,
                ActivatingHandlers,
                ActivatedHandlers));

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
    }
}
