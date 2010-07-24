using System.Collections.Generic;
using Autofac;
using Autofac.Core;

namespace AutofacContrib.Profiling
{
    class DependencyTrackingContext : IComponentContext
    {
        readonly ContainerProfile _profile;
        readonly IComponentRegistration _registration;
        readonly IComponentContext _context;

        public DependencyTrackingContext(
            ContainerProfile profile,
            IComponentRegistration registration,
            IComponentContext context)
        {
            _profile = profile;
            _registration = registration;
            _context = context;
        }

        public object Resolve(IComponentRegistration registration, IEnumerable<Parameter> parameters)
        {
            _profile.RecordDependency(_registration, registration);
            return _context.Resolve(registration, parameters);
        }

        public IComponentRegistry ComponentRegistry
        {
            get { return _context.ComponentRegistry; }
        }
    }
}