using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Autofac;
using Autofac.Configuration;
using Calculator.Api;
using System.Threading;

namespace Calculator
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                var builder = new ContainerBuilder();
                
                builder.RegisterType<CalculatorForm>();

                builder.RegisterType<Calculator>();

                builder.RegisterType<MessageBoxNotifier>()
                    .As<INotifier>();
                
                builder.RegisterCollection<IOperation>()
                    .As<IEnumerable<IOperation>>()
                    .Named("operations");

                builder.RegisterModule(new ConfigurationSettingsReader("calculator"));
                
                using (var container = builder.Build())
                {
                    Application.ThreadException += Application_ThreadException;
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(container.Resolve<CalculatorForm>());
                }
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            DisplayException(e.Exception);
        }

        private static void DisplayException(Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
}
