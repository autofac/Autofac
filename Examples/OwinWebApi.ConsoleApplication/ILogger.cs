namespace OwinWebApi.ConsoleApplication
{
    public interface ILogger
    {
        void Write(string message, params object[] args);
    }
}