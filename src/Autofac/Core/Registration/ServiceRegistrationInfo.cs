// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// https://autofac.org
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Tracks the services known to the registry.
    /// </summary>
    internal class ServiceRegistrationInfo
    {
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "The _service field is useful in debugging and diagnostics.")]
        private readonly Service _service;

        /// <summary>
        ///  List of implicit default service implementations. Overriding default implementations are appended to the end,
        ///  so the enumeration should begin from the end too, and the most default implementation comes last.
        ///  Used from multithreaded environment, use Interlocked/Volatile for read and write new List each time.
        /// </summary>
        private List<IComponentRegistration> _defaultImplementations = new List<IComponentRegistration>();

        /// <summary>
        ///  List of service implementations coming from sources. Sources have priority over preserve-default implementations.
        ///  Implementations from sources are enumerated in preserve-default order, so the most default implementation comes first.
        ///  Used from multithreaded environment, use Interlocked/Volatile for read and write new List each time.
        /// </summary>
        private List<IComponentRegistration> _sourceImplementations = null;

        /// <summary>
        ///  List of explicit service implementations specified with the PreserveExistingDefaults option.
        ///  Enumerated in preserve-defaults order, so the most default implementation comes first.
        ///  Used from multithreaded environment, use Interlocked/Volatile for read and write new List each time.
        /// </summary>
        private List<IComponentRegistration> _preserveDefaultImplementations = null;

        /// <summary>
        ///  Used from multithreaded environment, use Interlocked/Volatile for read and write each time.
        /// </summary>
        [SuppressMessage("Microsoft.ApiDesignGuidelines", "CA2213", Justification = "The creator of the compponent registration is responsible for disposal.")]
        private IComponentRegistration _defaultImplementation;

        /// <summary>
        /// Used for bookkeeping so that the same source is not queried twice (may be null).
        /// </summary>
        private Queue<IRegistrationSource> _sourcesToQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRegistrationInfo"/> class.
        /// </summary>
        /// <param name="service">The tracked service.</param>
        public ServiceRegistrationInfo(Service service)
        {
            _service = service;
        }

        private volatile bool _isInitialized;

        /// <summary>
        /// Gets a value indicating whether the first time a service is requested, initialization (e.g. reading from sources)
        /// happens. This value will then be set to true. Calling many methods on this type before
        /// initialisation is an error.
        /// </summary>
        public bool IsInitialized
        {
            get
            {
                return _isInitialized;
            }

            private set
            {
                _isInitialized = value;
            }
        }

        /// <summary>
        /// Gets the known implementations. The first implementation is a default one.
        /// </summary>
        public IEnumerable<IComponentRegistration> Implementations
        {
            get
            {
                RequiresInitialization();
                var resultingCollection = Enumerable.Reverse(Volatile.Read(ref _defaultImplementations));
                var sourceImplementations = Volatile.Read(ref _sourceImplementations);
                if (sourceImplementations != null)
                {
                    resultingCollection = resultingCollection.Concat(sourceImplementations);
                }

                var preserveDefaultImplementations = Volatile.Read(ref _preserveDefaultImplementations);
                if (preserveDefaultImplementations != null)
                {
                    resultingCollection = resultingCollection.Concat(preserveDefaultImplementations);
                }

                return resultingCollection;
            }
        }

        private void RequiresInitialization()
        {
            if (!IsInitialized)
                throw new InvalidOperationException(ServiceRegistrationInfoResources.NotInitialized);
        }

        /// <summary>
        /// Gets a value indicating whether any implementations are known.
        /// </summary>
        public bool IsRegistered
        {
            get
            {
                RequiresInitialization();
                return Any;
            }
        }

        private bool Any =>
            Volatile.Read(ref _defaultImplementations).Count > 0 ||
            Volatile.Read(ref _sourceImplementations) != null ||
            Volatile.Read(ref _preserveDefaultImplementations) != null;

        public void AddImplementation(IComponentRegistration registration, bool preserveDefaults, bool originatedFromSource)
        {
            if (preserveDefaults)
            {
                if (originatedFromSource)
                {
                    // called inside of lock, just making List _sourceImplementations thread safe
                    if (Volatile.Read(ref _sourceImplementations) == null)
                    {
                        Volatile.Write(ref _sourceImplementations, new List<IComponentRegistration>());
                    }

                    var newSourceImplementations = new List<IComponentRegistration>(Volatile.Read(ref _sourceImplementations));
                    newSourceImplementations.Add(registration);

                    Volatile.Write(ref _sourceImplementations, newSourceImplementations);
                }
                else
                {
                    // called inside of lock, just making List _preserveDefaultImplementations thread safe
                    if (Volatile.Read(ref _preserveDefaultImplementations) == null)
                    {
                        Volatile.Write(ref _preserveDefaultImplementations, new List<IComponentRegistration>());
                    }

                    var newPreserveDefaultImplementations = new List<IComponentRegistration>(Volatile.Read(ref _preserveDefaultImplementations));
                    newPreserveDefaultImplementations.Add(registration);

                    Volatile.Write(ref _preserveDefaultImplementations, newPreserveDefaultImplementations);
                }
            }
            else
            {
                if (originatedFromSource)
                    throw new ArgumentOutOfRangeException(nameof(originatedFromSource));

                var newDefaultImplementations = new List<IComponentRegistration>(Volatile.Read(ref _defaultImplementations));
                newDefaultImplementations.Add(registration);

                Volatile.Write(ref _defaultImplementations, newDefaultImplementations);
            }

            Volatile.Write(ref _defaultImplementation, null);
        }

        public bool TryGetRegistration(out IComponentRegistration registration)
        {
            RequiresInitialization();

            registration = Volatile.Read(ref _defaultImplementation);
            if (registration != null)
                return true;

            var defaultImplementations = Volatile.Read(ref _defaultImplementations);
            if (defaultImplementations.Count > 0)
            {
                registration = defaultImplementations[defaultImplementations.Count - 1];
                Volatile.Write(ref _defaultImplementation, registration);
                return true;
            }

            var sourceImplementations = Volatile.Read(ref _sourceImplementations);
            if (sourceImplementations != null && sourceImplementations.Count > 0)
            {
                registration = sourceImplementations[0];
                Volatile.Write(ref _defaultImplementation, registration);
                return true;
            }

            var preserveDefaultImplementations = Volatile.Read(ref _preserveDefaultImplementations);
            if (preserveDefaultImplementations != null && preserveDefaultImplementations.Count > 0)
            {
                registration = preserveDefaultImplementations[0];
                Volatile.Write(ref _defaultImplementation, registration);
                return true;
            }

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

        private void EnforceDuringInitialization()
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

            // IMPROVEMENT - We only need to check service registration infos that:
            // - Have already been initialized
            // - Were created via a registration source (because we might be adding an equivalent explicit registration such as Func<T>)
            // - Don't contain any registrations (because a registration source was added when no adaptee was present)
            return IsInitialized && (Volatile.Read(ref _sourceImplementations) != null || !Any);
        }
    }
}