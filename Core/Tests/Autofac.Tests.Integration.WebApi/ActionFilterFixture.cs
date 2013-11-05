using System;
using System.Web.Http.Filters;
using Autofac.Builder;
using Autofac.Integration.WebApi;

namespace Autofac.Tests.Integration.WebApi
{
    public class ActionFilterFixture : AutofacFilterBaseFixture<TestActionFilter, TestActionFilter2, IActionFilter>
    {
        protected override Func<IComponentContext, TestActionFilter> GetFirstRegistration()
        {
            return c => new TestActionFilter(c.Resolve<ILogger>());
        }

        protected override Func<IComponentContext, TestActionFilter2> GetSecondRegistration()
        {
            return c => new TestActionFilter2(c.Resolve<ILogger>());
        }

        protected override Action<IRegistrationBuilder<TestActionFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstControllerRegistration()
        {
            return r => r.AsWebApiActionFilterFor<TestController>();
        }

        protected override Action<IRegistrationBuilder<TestActionFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstActionRegistration()
        {
            return r => r.AsWebApiActionFilterFor<TestController>(c => c.Get());
        }

        protected override Action<IRegistrationBuilder<TestActionFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondControllerRegistration()
        {
            return r => r.AsWebApiActionFilterFor<TestController>();
        }

        protected override Action<IRegistrationBuilder<TestActionFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondActionRegistration()
        {
            return r => r.AsWebApiActionFilterFor<TestController>(c => c.Get());
        }

        protected override Type GetWrapperType()
        {
            return typeof(ActionFilterWrapper);
        }

        protected override Action<ContainerBuilder> ConfigureControllerFilterOverride()
        {
            return builder => builder.OverrideWebApiActionFilterFor<TestController>();
        }

        protected override Action<ContainerBuilder> ConfigureActionFilterOverride()
        {
            return builder => builder.OverrideWebApiActionFilterFor<TestController>(c => c.Get());
        }
    }
}