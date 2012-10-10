using System;
using Autofac;
using Autofac.Core;

namespace Autofac.Extras.Attributed
{
    /// <summary>
    /// this module will scan all registrations for metadata and associate them if found
    /// </summary>
    public class AttributedMetadataModule : Module
    {
        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException("registration");
            }
            foreach (var property in MetadataHelper.GetMetadata(registration.Activator.LimitType))
                registration.Metadata.Add(property);
        }
    }
}
