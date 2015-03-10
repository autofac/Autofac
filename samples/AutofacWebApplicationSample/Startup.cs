using System;
using Autofac;
using Autofac.Dnx;
using AutofacTestWebApplication.Models;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Diagnostics.Entity;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Identity;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Logging.Console;

namespace AutofacTestWebApplication
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Setup configuration sources.
            Configuration = new Configuration()
                .AddJsonFile("config.json")
                .AddEnvironmentVariables();
        }

        public IConfiguration Configuration { get; set; }

        // This method gets called by the runtime.
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerfactory)
        {
            app.UseServices(services =>
            {
                // Add EF services to the services container.
                services.AddEntityFramework(Configuration)
                    .AddSqlServer()
                    .AddDbContext<ApplicationDbContext>();

                // Add Identity services to the services container.
                services.AddIdentity<ApplicationUser, IdentityRole>(Configuration)
                    .AddEntityFrameworkStores<ApplicationDbContext>();

                // Add MVC services to the services container.
                services.AddMvc();

                // Uncomment the following line to add Web API servcies which makes it easier to port Web API 2 controllers.
                // You need to add Microsoft.AspNet.Mvc.WebApiCompatShim package to project.json
                // services.AddWebApiConventions();

                // Create the Autofac container builder.
                var builder = new ContainerBuilder();

                // Add any Autofac modules or registrations.
                builder.RegisterModule(new AutofacModule());

                // Populate the services.
                builder.Populate(services);

                // Build the container.
                var container = builder.Build();

                // Resolve and return the service provider.
                return container.Resolve<IServiceProvider>();
            });

            // Configure the HTTP request pipeline.
            // Add the console logger.
            loggerfactory.AddConsole();

            // Add the following to the request pipeline only in development environment.
            if (string.Equals(env.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase))
            {
                app.UseBrowserLink();
                app.UseErrorPage(ErrorPageOptions.ShowAll);
                app.UseDatabaseErrorPage(DatabaseErrorPageOptions.ShowAll);
            }
            else
            {
                // Add Error handling middleware which catches all application specific errors and
                // send the request to the following path or controller action.
                app.UseErrorHandler("/Home/Error");
            }

            // Add static files to the request pipeline.
            app.UseStaticFiles();

            // Add cookie-based authentication to the request pipeline.
            app.UseIdentity();

            // Add MVC to the request pipeline.
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" });

                // Uncomment the following line to add a route for porting Web API 2 controllers.
                // routes.MapWebApiRoute("DefaultApi", "api/{controller}/{id?}");
            });
        }
    }
}
