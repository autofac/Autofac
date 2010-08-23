using Autofac;

namespace AutofacContrib.Attributed
{ 
    /// <summary>
    /// this module will scan all registrations for metadata and associate them if found
    /// </summary>
    public abstract class AttributedMetadataModule : Module
    {
        protected override void AttachToComponentRegistration(Autofac.Core.IComponentRegistry componentRegistry, Autofac.Core.IComponentRegistration registration)
        {
            foreach( var property in MetadataHelper.GetMetadata(registration.Activator.LimitType))
                registration.Metadata.Add(property);
        }
    }
}
