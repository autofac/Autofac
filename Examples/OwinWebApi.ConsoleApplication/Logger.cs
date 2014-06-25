using System.Diagnostics;

namespace OwinWebApi.ConsoleApplication
{
    public class Logger : ILogger
    {
        public void Write(string message, params object[] args)
        {
            Debug.WriteLine(message, args);
        }
    }
}
