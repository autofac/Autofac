using System;
using System.Web.Http.Filters;
using Autofac.Builder;
using Autofac.Integration.WebApi;

namespace Autofac.Tests.Integration.WebApi
{
    public class AuthorizationFilterFixture : AutofacFilterBaseFixture<TestAuthorizationFilter, TestAuthorizationFilter2, IAuthorizationFilter>
    {
        protected override Func<IComponentContext, TestAuthorizationFilter> GetFirstRegistration()
        {
            return c => new TestAuthorizationFilter(c.Resolve<ILogger>());
        }

        protected override Func<IComponentContext, TestAuthorizationFilter2> GetSecondRegistration()
        {
            return c => new TestAuthorizationFilter2(c.Resolve<ILogger>());
        }

        protected override Action<IRegistrationBuilder<TestAuthorizationFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstControllerRegistration()
        {
            return r => r.AsWebApiAuthorizationFilterFor<TestController>();
        }

        protected override Action<IRegistrationBuilder<TestAuthorizationFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstActionRegistration()
        {
            return r => r.AsWebApiAuthorizationFilterFor<TestController>(c => c.Get());
        }

        protected override Action<IRegistrationBuilder<TestAuthorizationFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondControllerRegistration()
        {
            return r => r.AsWebApiAuthorizationFilterFor<TestController>();
        }

        protected override Action<IRegistrationBuilder<TestAuthorizationFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondActionRegistration()
        {
            return r => r.AsWebApiAuthorizationFilterFor<TestController>(c => c.Get());
        }

        protected override Type GetWrapperType()
        {
            return typeof(AuthorizationFilterWrapper);
        }

        protected override Action<ContainerBuilder> ConfigureControllerFilterOverride()
        {
            return builder => builder.OverrideWebApiAuthorizationFilterFor<TestController>();
        }

        protected override Action<ContainerBuilder> ConfigureActionFilterOverride()
        {
            return builder => builder.OverrideWebApiAuthorizationFilterFor<TestController>(c => c.Get());
        }
    }
}