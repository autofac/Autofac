using System;
using Autofac;
using AutofacContrib.Multitenant;

namespace MultitenantExample.ConsoleApplication
{
    /// <summary>
    /// Demonstration console application illustrating multitenant dependency injection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// While there is not much of a use case for a console application to be
    /// multitenant, what this illustrates is that non-ASP.NET, non-WCF applications
    /// can also be multitenant. Windows services, for example, could make use
    /// of multitenancy.
    /// </para>
    /// </remarks>
    public class Program
    {
        /// <summary>
        /// The container from which dependencies will be resolved.
        /// </summary>
        private static IContainer _container;

        /// <summary>
        /// Strategy used for identifying the current tenant with multitenant DI.
        /// </summary>
        private static ManualTenantIdentificationStrategy _tenantIdentifier;

        /// <summary>
        /// Demo program entry point.
        /// </summary>
        public static void Main()
        {
            // Initialize the tenant identification strategy.
            _tenantIdentifier = new ManualTenantIdentificationStrategy();

            // Set the application container to the multitenant container.
            _container = ConfigureDependencies();

            // Explain what you're looking at.
            WriteInstructions();

            // Start listening for input.
            ListenForInput();
        }

        /// <summary>
        /// Configures the multitenant dependency container.
        /// </summary>
        private static IContainer ConfigureDependencies()
        {
            // Register default dependencies in the application container.
            var builder = new ContainerBuilder();
            builder.RegisterType<Consumer>().As<IDependencyConsumer>().InstancePerDependency();
            builder.RegisterType<BaseDependency>().As<IDependency>().SingleInstance();
            var appContainer = builder.Build();

            // Create the multitenant container.
            var mtc = new MultitenantContainer(_tenantIdentifier, appContainer);

            // Configure overrides for tenant 1. Tenant 1 registers their dependencies
            // as instance-per-dependency.
            mtc.ConfigureTenant('1', b => b.RegisterType<Tenant1Dependency>().As<IDependency>().InstancePerDependency());

            // Configure overrides for tenant 2. Tenant 2 registers their dependencies
            // as singletons.
            mtc.ConfigureTenant('2', b => b.RegisterType<Tenant2Dependency>().As<IDependency>().SingleInstance());

            // Configure overrides for the default tenant. That means the default
            // tenant will have some different dependencies than other unconfigured
            // tenants.
            mtc.ConfigureTenant(null, b => b.RegisterType<DefaultTenantDependency>().As<IDependency>().SingleInstance());

            return mtc;
        }

        /// <summary>
        /// Loops and listens for input until the user quits.
        /// </summary>
        private static void ListenForInput()
        {
            ConsoleKeyInfo input;
            do
            {
                Console.Write("Select a tenant (1-9, 0=default) or 'q' to quit: ");
                input = Console.ReadKey();
                Console.WriteLine();

                if (input.KeyChar >= 48 && input.KeyChar <= 57)
                {
                    // Set the "contextual" tenant ID based on the input, then
                    // put it to the test.
                    _tenantIdentifier.CurrentTenantId = input.KeyChar;
                    ResolveDependencyAndWriteInfo();
                }
                else if (input.Key != ConsoleKey.Q)
                {
                    Console.WriteLine("Invalid key pressed.");
                }
            } while (input.Key != ConsoleKey.Q);
        }

        /// <summary>
        /// Resolves the dependency from the container and displays some information
        /// about it.
        /// </summary>
        private static void ResolveDependencyAndWriteInfo()
        {
            var consumer = _container.Resolve<IDependencyConsumer>();
            Console.WriteLine("Tenant ID:       {0}", _tenantIdentifier.CurrentTenantId);
            Console.WriteLine("Dependency Type: {0}", consumer.Dependency.GetType().Name);
            Console.WriteLine("Instance ID:     {0}", consumer.Dependency.InstanceId);
            Console.WriteLine();
        }

        /// <summary>
        /// Writes the application instructions to the screen.
        /// </summary>
        private static void WriteInstructions()
        {
            Console.WriteLine("Multitenant Example: Console Application");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("Select a tenant ID (1 - 9) to see the dependencies that get resolved for that tenant. You will see the dependency type as well as the instance ID so you can verify the proper type and lifetime scope registration is being used.");
            Console.WriteLine();
            Console.WriteLine("* Tenant 1 has an override registered as InstancePerDependency.");
            Console.WriteLine("* Tenant 2 has an override registered as SingleInstance.");
            Console.WriteLine("* The default tenant has an override registered as SingleInstance.");
            Console.WriteLine("* Tenants that don't have overrides fall back to the application/base singleton.");
            Console.WriteLine();
        }
    }
}
