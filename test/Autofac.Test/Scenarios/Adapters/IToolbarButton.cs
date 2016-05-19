namespace Autofac.Test.Scenarios.Adapters
{
    public interface IToolbarButton
    {
        string Name { get; }

        Command Command { get; }
    }
}