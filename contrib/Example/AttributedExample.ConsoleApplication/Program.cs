using System;
using AttributedExample.ConsoleApplication.StateTypes;
using Autofac;

namespace AttributedExample.ConsoleApplication
{
    public class IocConfiguration
    {
        public static void Configure(ContainerBuilder componentRegistry)
        {
            componentRegistry.RegisterType<StateEngine>();
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();

            IocConfiguration.Configure(builder);
            
            var container = builder.Build();
            

            var mainMenu = new MainMenu();
            DocumentType? option = null;

            while((option = mainMenu.Loop())!= null)
            {
                var stateEngine = container.Resolve<Func<DocumentType, StateEngine>>()(option.Value);

            }
        }
    }
}
