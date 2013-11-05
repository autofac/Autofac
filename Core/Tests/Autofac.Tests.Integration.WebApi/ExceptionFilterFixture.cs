using System;
using System.Web.Http.Filters;
using Autofac.Builder;
using Autofac.Integration.WebApi;

namespace Autofac.Tests.Integration.WebApi
{
    public class ExceptionFilterFixture : AutofacFilterBaseFixture<TestExceptionFilter, TestExceptionFilter2, IExceptionFilter>
    {
        protected override Func<IComponentContext, TestExceptionFilter> GetFirstRegistration()
        {
            return c => new TestExceptionFilter(c.Resolve<ILogger>());
        }

        protected override Func<IComponentContext, TestExceptionFilter2> GetSecondRegistration()
        {
            return c => new TestExceptionFilter2(c.Resolve<ILogger>());
        }

        protected override Action<IRegistrationBuilder<TestExceptionFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstControllerRegistration()
        {
            return r => r.AsWebApiExceptionFilterFor<TestController>();
        }

        protected override Action<IRegistrationBuilder<TestExceptionFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstActionRegistration()
        {
            return r => r.AsWebApiExceptionFilterFor<TestController>(c => c.Get());
        }

        protected override Action<IRegistrationBuilder<TestExceptionFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondControllerRegistration()
        {
            return r => r.AsWebApiExceptionFilterFor<TestController>();
        }

        protected override Action<IRegistrationBuilder<TestExceptionFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondActionRegistration()
        {
            return r => r.AsWebApiExceptionFilterFor<TestController>(c => c.Get());
        }

        protected override Type GetWrapperType()
        {
            return typeof(ExceptionFilterWrapper);
        }

        protected override Action<ContainerBuilder> ConfigureControllerFilterOverride()
        {
            return builder => builder.OverrideWebApiExceptionFilterFor<TestController>();
        }

        protected override Action<ContainerBuilder> ConfigureActionFilterOverride()
        {
            return builder => builder.OverrideWebApiExceptionFilterFor<TestController>(c => c.Get());
        }
    }
}