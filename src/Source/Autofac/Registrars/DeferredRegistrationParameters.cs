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

namespace Autofac.Registrars
{
    /// <summary>
    /// Parameters that allow a registration to be created at a later point when one
    /// is required.
    /// </summary>
    public class DeferredRegistrationParameters
    {
        InstanceOwnership _ownership;
        InstanceScope _scope;
        IEnumerable<EventHandler<PreparingEventArgs>> _preparingHandlers;
        IEnumerable<EventHandler<ActivatingEventArgs>> _activatingHandlers;
        IEnumerable<EventHandler<ActivatedEventArgs>> _activatedHandlers;
        RegistrationCreator _createRegistration;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeferredRegistrationParameters"/> class.
        /// </summary>
        /// <param name="ownership">The ownership.</param>
        /// <param name="scope">The scope.</param>
        /// <param name="preparingHandlers">The preparing handlers.</param>
        /// <param name="activatingHandlers">The activating handlers.</param>
        /// <param name="activatedHandlers">The activated handlers.</param>
        /// <param name="createRegistration">The create registration.</param>
        public DeferredRegistrationParameters(
            InstanceOwnership ownership,
            InstanceScope scope,
            IEnumerable<EventHandler<PreparingEventArgs>> preparingHandlers,
            IEnumerable<EventHandler<ActivatingEventArgs>> activatingHandlers,
            IEnumerable<EventHandler<ActivatedEventArgs>> activatedHandlers,
            RegistrationCreator createRegistration)
        {
            _ownership = ownership;
            _scope = scope;
            _preparingHandlers = Enforce.ArgumentNotNull(preparingHandlers, "preparingHandlers");
            _activatingHandlers = Enforce.ArgumentNotNull(activatingHandlers, "activatingHandlers");
            _activatedHandlers = Enforce.ArgumentNotNull(activatedHandlers, "activatedHandlers");
            _createRegistration = Enforce.ArgumentNotNull(createRegistration, "createRegistration");
        }

        /// <summary>
        /// Gets the ownership.
        /// </summary>
        /// <value>The ownership.</value>
        public InstanceOwnership Ownership
        {
            get
            {
                return _ownership;
            }
        }

        /// <summary>
        /// Gets the scope.
        /// </summary>
        /// <value>The scope.</value>
        public InstanceScope Scope
        {
            get
            {
                return _scope;
            }
        }

        /// <summary>
        /// Gets the activating handlers.
        /// </summary>
        /// <value>The handlers.</value>
        public IEnumerable<EventHandler<ActivatingEventArgs>> ActivatingHandlers
        {
            get
            {
                return _activatingHandlers;
            }
        }

        /// <summary>
        /// Gets the preparing handlers.
        /// </summary>
        /// <value>The handlers.</value>
        public IEnumerable<EventHandler<PreparingEventArgs>> PreparingHandlers
        {
            get
            {
                return _preparingHandlers;
            }
        }

        /// <summary>
        /// Gets the activated handlers.
        /// </summary>
        /// <value>The handlers.</value>
        public IEnumerable<EventHandler<ActivatedEventArgs>> ActivatedHandlers
        {
            get
            {
                return _activatedHandlers;
            }
        }

        /// <summary>
        /// Gets the registration creator.
        /// </summary>
        /// <value>The registration creator.</value>
        public RegistrationCreator RegistrationCreator
        {
            get
            {
                return _createRegistration;
            }
        }
    }
}
