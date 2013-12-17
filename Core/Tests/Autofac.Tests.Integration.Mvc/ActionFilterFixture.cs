using System;
using System.Web.Mvc;
using Autofac.Builder;
using Autofac.Integration.Mvc;

namespace Autofac.Tests.Integration.Mvc
{
    public class ActionFilterFixture : AutofacFilterBaseFixture<TestActionFilter, TestActionFilter2, IActionFilter>
    {
        protected override Action<IRegistrationBuilder<TestActionFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstControllerRegistration()
        {
            return r => r.AsActionFilterFor<TestController>();
        }

        protected override Action<IRegistrationBuilder<TestActionFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstActionRegistration()
        {
            return r => r.AsActionFilterFor<TestController>(c => c.Action1(default(string)));
        }

        protected override Action<IRegistrationBuilder<TestActionFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondControllerRegistration()
        {
            return r => r.AsActionFilterFor<TestController>(20);
        }

        protected override Action<IRegistrationBuilder<TestActionFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondActionRegistration()
        {
            return r => r.AsActionFilterFor<TestController>(c => c.Action1(default(string)), 20);
        }

        protected override Action<ContainerBuilder> ConfigureControllerFilterOverride()
        {
            return builder => builder.OverrideActionFilterFor<TestController>();
        }

        protected override Action<ContainerBuilder> ConfigureActionFilterOverride()
        {
            return builder => builder.OverrideActionFilterFor<TestController>(c => c.Action1(default(string)));
        }

        protected override Action<IRegistrationBuilder<TestActionFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureActionOverrideRegistration()
        {
            return r => r.AsActionFilterOverrideFor<TestController>(c => c.Action1(default(string)));
        }

        protected override Action<IRegistrationBuilder<TestActionFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureControllerOverrideRegistration()
        {
            return r => r.AsActionFilterOverrideFor<TestController>();
        }

        protected override Type GetWrapperType()
        {
            return typeof(ActionFilterOverride);
        }
    }
}