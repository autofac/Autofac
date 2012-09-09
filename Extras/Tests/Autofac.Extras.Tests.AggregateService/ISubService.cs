namespace Autofac.Extras.Tests.AggregateService
{
    public interface ISubService : ISuperService
    {
        ISomeOtherDependency SomeOtherDependency { get; }
    }
}
