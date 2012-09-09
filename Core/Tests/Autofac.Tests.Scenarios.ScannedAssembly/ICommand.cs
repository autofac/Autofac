namespace Autofac.Tests.Scenarios.ScannedAssembly
{
    /// <summary>
    /// An open generic interface type.
    /// </summary>
    public interface ICommand<T>
    {
        void Execute(T data);
    }
}
