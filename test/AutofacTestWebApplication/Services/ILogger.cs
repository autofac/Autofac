namespace AutofacTestWebApplication.Services
{
    public interface ILogger
    {
        void Log(string message, params object[] arguments);
    }
}