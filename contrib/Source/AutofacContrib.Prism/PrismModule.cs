using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Autofac;
using Autofac.Builder;
using Microsoft.Practices.Composite;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Composite.Wpf.Regions;

namespace AutofacContrib.Prism
{
    public class PrismModule : Autofac.Builder.Module
    {
        Type _shellType;

        public PrismModule(Type shellType)
        {
            _shellType = shellType;
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.SetDefaultOwnership(InstanceOwnership.External);

            ConfigureContainer(builder);

            builder.Register(_shellType)
                .OnPreparing((s, e) =>
                {
                    RegionAdapterMappings regionAdapterMappings = e.Context.ResolveOptional<RegionAdapterMappings>();
                    if (regionAdapterMappings != null)
                    {
                        regionAdapterMappings.RegisterMapping(typeof(Selector), new SelectorRegionAdapter());
                        regionAdapterMappings.RegisterMapping(typeof(ItemsControl), new ItemsControlRegionAdapter());
                        regionAdapterMappings.RegisterMapping(typeof(ContentControl), new ContentControlRegionAdapter());
                    }
                })
                .OnActivated((s, e) =>
                {
                    DependencyObject shell = (DependencyObject)e.Instance;
                    RegionManager.SetRegionManager(shell, e.Context.Resolve<IRegionManager>());
                    InitializeModules(e.Context);
                });
        }

        void ConfigureContainer(ContainerBuilder builder)
        {
            builder.Register<TraceLogger>().As<ILoggerFacade>().DefaultOnly();
            builder.Register<ConfigurationModuleEnumerator>().As<IModuleEnumerator>().DefaultOnly();
            builder.Register<AutofacContainerAdapter>().As<IContainerFacade>().DefaultOnly();
            builder.Register<EventAggregator>().As<IEventAggregator>().DefaultOnly();
            builder.Register<RegionAdapterMappings>().As<RegionAdapterMappings>().DefaultOnly();
            builder.Register<RegionManager>().As<IRegionManager>().DefaultOnly();
            builder.Register<ModuleLoader>().As<IModuleLoader>().DefaultOnly();
        }

        void InitializeModules(IContext c)
        {
            IModuleEnumerator moduleEnumerator = c.Resolve<IModuleEnumerator>();
            IModuleLoader moduleLoader = c.Resolve<IModuleLoader>();
            ModuleInfo[] moduleInfo = moduleEnumerator.GetStartupLoadedModules();
            moduleLoader.Initialize(moduleInfo);
        }
    }
}