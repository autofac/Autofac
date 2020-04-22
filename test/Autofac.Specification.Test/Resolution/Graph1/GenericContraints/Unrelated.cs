namespace Autofac.Test.Scenarios.Graph1.GenericContraints
{
    public class Unrelated<T> : IB<T>
        where T : class, new()
    {
    }
}
