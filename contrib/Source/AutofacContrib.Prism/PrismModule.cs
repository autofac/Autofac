using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Microsoft.Practices.Composite;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Composite.Wpf.Regions;

namespace AutofacContrib.Prism
{
    public class PrismModule : Autofac.Module
    {
        readonly Type _shellType;

        public PrismModule(Type shellType)
        {
            _shellType = shellType;
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            ConfigureContainer(builder);

            builder.RegisterType(_shellType)
                .ExternallyOwned()
                .OnPreparing(e =>
                {
                    var regionAdapterMappings = e.Context.ResolveOptional<RegionAdapterMappings>();
                    if (regionAdapterMappings != null)
                    {
                        regionAdapterMappings.RegisterMapping(typeof(Selector), new SelectorRegionAdapter());
                        regionAdapterMappings.RegisterMapping(typeof(ItemsControl), new ItemsControlRegionAdapter());
                        regionAdapterMappings.RegisterMapping(typeof(ContentControl), new ContentControlRegionAdapter());
                    }
                })
                .OnActivated(e =>
                {
                    var shell = (DependencyObject)e.Instance;
                    RegionManager.SetRegionManager(shell, e.Context.Resolve<IRegionManager>());
                    InitializeModules(e.Context);
                });
        }

        static void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterType<TraceLogger>().As<ILoggerFacade>().ExternallyOwned().PreserveExistingDefaults();
            builder.RegisterType<ConfigurationModuleEnumerator>().As<IModuleEnumerator>().ExternallyOwned().PreserveExistingDefaults();
            builder.RegisterType<AutofacContainerAdapter>().As<IContainerFacade>().ExternallyOwned().PreserveExistingDefaults();
            builder.RegisterType<EventAggregator>().As<IEventAggregator>().ExternallyOwned().PreserveExistingDefaults();
            builder.RegisterType<RegionAdapterMappings>().As<RegionAdapterMappings>().ExternallyOwned().PreserveExistingDefaults();
            builder.RegisterType<RegionManager>().As<IRegionManager>().ExternallyOwned().PreserveExistingDefaults();
            builder.RegisterType<ModuleLoader>().As<IModuleLoader>().ExternallyOwned().PreserveExistingDefaults();
        }

        static void InitializeModules(IComponentContext c)
        {
            var moduleEnumerator = c.Resolve<IModuleEnumerator>();
            var moduleLoader = c.Resolve<IModuleLoader>();
            var moduleInfo = moduleEnumerator.GetStartupLoadedModules();
            moduleLoader.Initialize(moduleInfo);
        }
    }
}