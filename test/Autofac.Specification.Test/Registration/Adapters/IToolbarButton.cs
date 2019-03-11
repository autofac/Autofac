namespace Autofac.Specification.Test.Registration.Adapters
{
    public interface IToolbarButton
    {
        string Name { get; }

        Command Command { get; }
    }
}