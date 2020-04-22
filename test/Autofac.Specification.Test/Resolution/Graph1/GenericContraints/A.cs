namespace Autofac.Test.Scenarios.Graph1.GenericContraints
{
    public class A : IA
    {
        public A(IB<ClassWithParameterlessButNotPublicConstructor> b)
        {
        }
    }
}
