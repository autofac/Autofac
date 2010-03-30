namespace Autofac.Tests.Scenarios.Adapters
{
    interface IToolbarButton
    {
        string Name { get; }
        Command Command { get; }
    }
}