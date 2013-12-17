using System;
using System.Web.Http.Filters;
using Autofac.Builder;
using Autofac.Integration.WebApi;

namespace Autofac.Tests.Integration.WebApi
{
    public class AuthenticationFilterFixture : AutofacFilterBaseFixture<TestAuthenticationFilter, TestAuthenticationFilter2, IAuthenticationFilter>
    {
        protected override Func<IComponentContext, TestAuthenticationFilter> GetFirstRegistration()
        {
            return c => new TestAuthenticationFilter(c.Resolve<ILogger>());
        }

        protected override Func<IComponentContext, TestAuthenticationFilter2> GetSecondRegistration()
        {
            return c => new TestAuthenticationFilter2(c.Resolve<ILogger>());
        }

        protected override Action<IRegistrationBuilder<TestAuthenticationFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstControllerRegistration()
        {
            return r => r.AsWebApiAuthenticationFilterFor<TestController>();
        }

        protected override Action<IRegistrationBuilder<TestAuthenticationFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstActionRegistration()
        {
            return r => r.AsWebApiAuthenticationFilterFor<TestController>(c => c.Get());
        }

        protected override Action<IRegistrationBuilder<TestAuthenticationFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondControllerRegistration()
        {
            return r => r.AsWebApiAuthenticationFilterFor<TestController>();
        }

        protected override Action<IRegistrationBuilder<TestAuthenticationFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondActionRegistration()
        {
            return r => r.AsWebApiAuthenticationFilterFor<TestController>(c => c.Get());
        }

        protected override Type GetWrapperType()
        {
            return typeof(AuthenticationFilterWrapper);
        }

        protected override Type GetOverrideWrapperType()
        {
            return typeof(AuthenticationFilterOverrideWrapper);
        }

        protected override Action<ContainerBuilder> ConfigureControllerFilterOverride()
        {
            return builder => builder.OverrideWebApiAuthenticationFilterFor<TestController>();
        }

        protected override Action<ContainerBuilder> ConfigureActionFilterOverride()
        {
            return builder => builder.OverrideWebApiAuthenticationFilterFor<TestController>(c => c.Get());
        }

        protected override Action<IRegistrationBuilder<TestAuthenticationFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureActionOverrideRegistration()
        {
            return builder => builder.AsWebApiAuthenticationFilterOverrideFor<TestController>(c => c.Get());
        }

        protected override Action<IRegistrationBuilder<TestAuthenticationFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureControllerOverrideRegistration()
        {
            return builder => builder.AsWebApiAuthenticationFilterOverrideFor<TestController>();
        }
    }
}