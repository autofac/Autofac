namespace Autofac.Tests.Scenarios.ScannedAssembly
{
    /// <summary>
    /// A command class that implements the open generic interface 
    /// type by inheriting from the abstract base class.
    /// </summary>
    public class DeleteCommand : CommandBase<DeleteCommandData>
    {
        public override void Execute(DeleteCommandData data)
        {
        }
    }
}
