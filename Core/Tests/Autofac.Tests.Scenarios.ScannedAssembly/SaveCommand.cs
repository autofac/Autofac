namespace Autofac.Tests.Scenarios.ScannedAssembly
{
    /// <summary>
    /// A command class that directly implements the open 
    /// generic interface type.
    /// </summary>
    public class SaveCommand : ICommand<SaveCommandData>
    {
        public void Execute(SaveCommandData data)
        {
        }
    }
}
