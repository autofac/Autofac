namespace Autofac.Test.Scenarios.GenericContraints
{
    public class A : IA
    {
        public A(IB<ClassWithParameterlessButNotPublicConstructor> b)
        {
        }
    }
}
