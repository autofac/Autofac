namespace Autofac.Test.Scenarios.ScannedAssembly
{
    /// <summary>
    /// An open generic interface type.
    /// </summary>
    public interface ICommand<T>
    {
        void Execute(T data);
    }
}
