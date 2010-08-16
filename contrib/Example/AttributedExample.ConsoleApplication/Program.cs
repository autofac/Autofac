using System;
using System.Reflection;
using AttributedExample.ConsoleApplication.Configuration;
using AttributedExample.ConsoleApplication.StateTypes;
using Autofac;
using AutofacContrib.Attributed;

namespace AttributedExample.ConsoleApplication
{
    /// <summary>
    /// a quick Ioc configuration separated to display the needed registrations
    /// </summary>
    public class IocConfiguration
    {
        public static void Configure(ContainerBuilder componentRegistry)
        {
            componentRegistry.RegisterType<StateEngine>();

            // the following registration hunts the listed assemblies for derivations of IStateStepConfiguration with marked MetadataAttribute-attributes
            // and converts these into strongly typed metadata.  also check out the alternate pattern used in the Attribute Tests scenario 4 where 
            // the Autofac Module pattern is extended for driving these types of registrations directly.
            componentRegistry.RegisterAssemblyTypedMetadata<IStateStepConfiguration, IStateStepConfigurationMetadata>(Assembly.GetExecutingAssembly());
        }
    }
     

    class Program
    {
        static void Main(string[] args)
        {
            // configure ioc
            var builder = new ContainerBuilder();

            IocConfiguration.Configure(builder);
            var container = builder.Build();
            
            var menu = new ApplicationMenu();
            DocumentType? option = null;

            // handle the menu loop until a non-selection is entered
            while((option = menu.DocumentSelectionLoop())!= null)
            {
                // using Service location to get us started with the document type selection
                var stateEngine = container.Resolve<Func<DocumentType, StateEngine>>()(option.Value);
                menu.ActionLoop(stateEngine);
            }
        }
    }
}
