namespace AutofacContrib.Tests.AggregateService
{
    public interface ISubService : ISuperService
    {
        ISomeOtherDependency SomeOtherDependency { get; }
    }
}
