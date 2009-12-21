namespace Autofac.Tests.Scenarios.ScannedAssembly
{
    /// <summary>
    /// An abstract base class that implements the open generic 
    /// interface type.
    /// </summary>
    public abstract class CommandBase<T> : ICommand<T>
    {
        public abstract void Execute(T data);
    }
}
