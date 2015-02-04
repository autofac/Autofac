namespace Autofac.Core.Registration
{
    static class ComponentRegistrationExtensions
    {
        public static bool IsAdapting(this IComponentRegistration componentRegistration)
        {
            return componentRegistration.Target != componentRegistration;
        }
    }
}
