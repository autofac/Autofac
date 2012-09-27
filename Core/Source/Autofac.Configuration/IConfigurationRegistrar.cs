using System;
using System.Collections.Generic;
using System.Linq;

namespace Autofac.Configuration
{
    public interface IConfigurationRegistrar
    {
        void RegisterConfigurationSection(ContainerBuilder builder, SectionHandler configurationSection);
    }
}
