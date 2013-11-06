using System;
using System.Web.Mvc;
using Autofac.Builder;
using Autofac.Integration.Mvc;

namespace Autofac.Tests.Integration.Mvc
{
    public class ResultFilterFixture : AutofacFilterBaseFixture<TestResultFilter, TestResultFilter2, IResultFilter>
    {
        protected override Action<IRegistrationBuilder<TestResultFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstControllerRegistration()
        {
            return r => r.AsResultFilterFor<TestController>();
        }

        protected override Action<IRegistrationBuilder<TestResultFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstActionRegistration()
        {
            return r => r.AsResultFilterFor<TestController>(c => c.Action1(default(string)));
        }

        protected override Action<IRegistrationBuilder<TestResultFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondControllerRegistration()
        {
            return r => r.AsResultFilterFor<TestController>(20);
        }

        protected override Action<IRegistrationBuilder<TestResultFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondActionRegistration()
        {
            return r => r.AsResultFilterFor<TestController>(c => c.Action1(default(string)), 20);
        }

        protected override Action<ContainerBuilder> ConfigureControllerFilterOverride()
        {
            return builder => builder.OverrideResultFilterFor<TestController>();
        }

        protected override Action<ContainerBuilder> ConfigureActionFilterOverride()
        {
            return builder => builder.OverrideResultFilterFor<TestController>(c => c.Action1(default(string)));
        }
    }
}