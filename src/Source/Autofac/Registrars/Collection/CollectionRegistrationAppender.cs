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


namespace Autofac.Registrars.Collection
{
	/// <summary>
	/// Appends services to a service collection.
	/// </summary>
	class CollectionRegistrationAppender
	{
		Service _collectionService;
		
		/// <summary>
		/// Create a new collection registration appender for the collection
		/// identified by <paramref name="collectionService"/>.
		/// </summary>
		/// <param name="collectionService">The collection.</param>
		public CollectionRegistrationAppender(Service collectionService)
		{
            _collectionService = Enforce.ArgumentNotNull(collectionService, "collectionService");
		}
		
        /// <summary>
        /// Adds the registration to a previously registered collection.
        /// </summary>
        /// <typeparam name="TRegistrar">The registrar's self-type.</typeparam>
        /// <param name="registrar">The registrar that will become a member of the collection.</param>
   		public void Add<TRegistrar>(IConcreteRegistrar<TRegistrar> registrar)
			where TRegistrar : IConcreteRegistrar<TRegistrar>
		{
            Enforce.ArgumentNotNull(registrar, "registrar");

            var collectionId = registrar.Id;
            registrar.OnRegistered((sender, e) =>
            {
                IDisposer disposer;
                IComponentRegistration serviceListRegistration;
                bool found = ((IRegistrationContext)e.Container)
                    .TryGetLocalRegistration(
                        _collectionService,
                        out serviceListRegistration,
                        out disposer);

                if (!found)
                    throw new ComponentNotRegisteredException(_collectionService);

                var serviceList = (IServiceListRegistration)serviceListRegistration;
                serviceList.Add(collectionId);
            });
		}
	}
}
