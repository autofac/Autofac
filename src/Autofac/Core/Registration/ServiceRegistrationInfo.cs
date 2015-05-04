// This software is part of the Autofac IoC container
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Tracks the services known to the registry.
    /// </summary>
    class ServiceRegistrationInfo
    {
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification="The _service field is useful in debugging and diagnostics.")]
        readonly Service _service;

        readonly LinkedList<IComponentRegistration> _implementations = new LinkedList<IComponentRegistration>();

        /// <summary>
        /// Used for bookkeeping so that the same source is not queried twice (may be null.)
        /// </summary>
        Queue<IRegistrationSource> _sourcesToQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRegistrationInfo"/> class.
        /// </summary>
        /// <param name="service">The tracked service.</param>
        public ServiceRegistrationInfo(Service service)
        {
            _service = service;
        }

        /// <summary>
        /// The first time a service is requested, initialization (e.g. reading from sources)
        /// happens. This value will then be set to true. Calling many methods on this type before
        /// initialisation is an error.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// The known implementations.
        /// </summary>
        public IEnumerable<IComponentRegistration> Implementations
        {
            get
            {
                RequiresInitialization();
                return _implementations;
            }
        }

        void RequiresInitialization()
        {
            if (!IsInitialized)
                throw new InvalidOperationException(ServiceRegistrationInfoResources.NotInitialized);
        }

        /// <summary>
        /// True if any implementations are known.
        /// </summary>
        public bool IsRegistered
        {
            get
            {
                RequiresInitialization();
                return Any;
            }
        }

        bool Any => _implementations.First != null;

        public void AddImplementation(IComponentRegistration registration, bool preserveDefaults)
        {
            if (preserveDefaults)
            {
                _implementations.AddLast(registration);
            }
            else
            {
                if (Any)
                    Debug.WriteLine(String.Format(
                        CultureInfo.InvariantCulture,
                        "[Autofac] Overriding default for: '{0}' with: '{1}' (was '{2}')",
                        _service, registration, _implementations.First.Value));

                _implementations.AddFirst(registration);
            }
        }

        public bool TryGetRegistration(out IComponentRegistration registration)
        {
            RequiresInitialization();

            if (Any)
            {
                registration = _implementations.First.Value;
                return true;
            }

            registration = null;
            return false;
        }

        public void Include(IRegistrationSource source)
        {
            if (IsInitialized)
                BeginInitialization(new[] { source });
            else if (IsInitializing)
                _sourcesToQuery.Enqueue(source);
        }

        public bool IsInitializing => !IsInitialized && _sourcesToQuery != null;

        public bool HasSourcesToQuery => IsInitializing && _sourcesToQuery.Count != 0;

        public void BeginInitialization(IEnumerable<IRegistrationSource> sources)
        {
            IsInitialized = false;
            _sourcesToQuery = new Queue<IRegistrationSource>(sources);
        }

        public void SkipSource(IRegistrationSource source)
        {
            EnforceDuringInitialization();

            _sourcesToQuery = new Queue<IRegistrationSource>(_sourcesToQuery.Where(rs => rs != source));
        }

        void EnforceDuringInitialization()
        {
            if (!IsInitializing)
                throw new InvalidOperationException(ServiceRegistrationInfoResources.NotDuringInitialization);
        }

        public IRegistrationSource DequeueNextSource()
        {
            EnforceDuringInitialization();

            return _sourcesToQuery.Dequeue();
        }

        public void CompleteInitialization()
        {
            // Does not EnforceDuringInitialization() because the recursive algorithm
            // sometimes completes initialisation at a deeper level than that which
            // began it.

            IsInitialized = true;
            _sourcesToQuery = null;
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "registration")]
        public bool ShouldRecalculateAdaptersOn(IComponentRegistration registration)
        {
            // The best optimisation we could make here is to track which services
            // have been queried by and adapter - i.e. instead of giving the
            // adapter the ComponentRegistry.RegistrationsFor method, add some
            // logic that performs the query then marks the service info as
            // an adapted on. Then only when registration supports services that
            // have been adapted do we need to do any querying at all.
            //
            // Once the optimization is in place, REMOVE THE SUPPRESSMESSAGE ATTRIBUTE
            // as, ostensibly, the actual registration will be used at that time.
            return IsInitialized;
        }
    }
}