using System.Diagnostics;
using System.Reflection;
using Autofac.Test;
using Xunit.Runners.UI;

namespace Autofac.Test.Uwp.DeviceRunner
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : RunnerApplication
    {
        protected override void OnInitializeRunner()
        {
            UnhandledException += (sender, args) => Debug.Write(args.Exception);

            // tests can be inside the main assembly
            //AddTestAssembly(GetType().GetTypeInfo().Assembly);
            // otherwise you need to ensure that the test assemblies will 
            // become part of the app bundle
            AddTestAssembly(typeof(ContainerBuilderTests).GetTypeInfo().Assembly);
        }
    }
}
