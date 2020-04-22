namespace Autofac.Test.Scenarios.GenericContraints
{
    public class Unrelated<T> : IB<T>
        where T : class, new()
    {
    }
}
