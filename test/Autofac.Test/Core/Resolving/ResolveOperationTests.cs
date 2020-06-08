using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Autofac.Core.Resolving;
using Xunit;

namespace Autofac.Test.Core.Resolving
{
    public class ResolveOperationTests
    {
        [Fact]
        public void AfterTheOperationIsFinished_ReusingTheTemporaryContextThrows()
        {
            IComponentContext ctx = null;
            var builder = new ContainerBuilder();
            builder.Register(c =>
            {
                ctx = c;
                return new object();
            });
            builder.RegisterInstance("Hello");
            var container = builder.Build();
            container.Resolve<string>();
            container.Resolve<object>();
            Assert.Throws<ObjectDisposedException>(() => ctx.Resolve<string>());
        }

        [Fact]
        public void OperationRaisesSuccessTraceEvents()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance("Hello");

            var container = builder.Build();

            var scope = container.Resolve<ILifetimeScope>() as ISharingLifetimeScope;

            var mockTracer = Mocks.GetTracer();

            var resolveOp = new ResolveOperation(scope, mockTracer);

            var raisedEvents = new List<string>();

            var request = new ResolveRequest(new TypedService(typeof(string)), scope.ResolvableImplementationFor<string>(), Enumerable.Empty<Parameter>());

            mockTracer.OperationStarting += (op, req) =>
            {
                raisedEvents.Add("opstart");
                Assert.Equal(resolveOp, op);
                Assert.Equal(request, req);
            };

            mockTracer.RequestStarting += (op, ctxt) =>
            {
                raisedEvents.Add("reqstart");
                Assert.Equal(resolveOp, op);
                Assert.Equal(request.Service, ctxt.Service);
            };

            mockTracer.RequestSucceeding += (op, ctxt) =>
            {
                raisedEvents.Add("reqsuccess");
                Assert.Equal(resolveOp, op);
            };

            mockTracer.OperationSucceeding += (op, instance) =>
            {
                raisedEvents.Add("opsuccess");
                Assert.Equal("Hello", instance);
            };

            resolveOp.Execute(request);

            Assert.Equal(new[] { "opstart", "reqstart", "reqsuccess", "opsuccess" }, raisedEvents);
        }

        [Fact]
        public void OperationRaisesFailureTraceEvents()
        {
            var builder = new ContainerBuilder();

            builder.Register<string>(ctxt => throw new InvalidOperationException());

            var container = builder.Build();

            var scope = container.Resolve<ILifetimeScope>() as ISharingLifetimeScope;

            var mockTracer = Mocks.GetTracer();

            var resolveOp = new ResolveOperation(scope, mockTracer);

            var raisedEvents = new List<string>();

            var request = new ResolveRequest(new TypedService(typeof(string)), scope.ResolvableImplementationFor<string>(), Enumerable.Empty<Parameter>());

            mockTracer.OperationStarting += (op, req) =>
            {
                raisedEvents.Add("opstart");
                Assert.Equal(resolveOp, op);
                Assert.Equal(request, req);
            };

            mockTracer.RequestStarting += (op, ctxt) =>
            {
                raisedEvents.Add("reqstart");
                Assert.Equal(resolveOp, op);
                Assert.Equal(request.Service, ctxt.Service);
            };

            mockTracer.RequestFailing += (op, ctxt, ex) =>
            {
                raisedEvents.Add("reqfail");
                Assert.Equal(resolveOp, op);
                Assert.IsType<DependencyResolutionException>(ex);
            };

            mockTracer.OperationFailing += (op, ex) =>
            {
                raisedEvents.Add("opfail");
                Assert.IsType<DependencyResolutionException>(ex);
            };

            try
            {
                resolveOp.Execute(request);
            }
            catch
            {
            }

            Assert.Equal(new[] { "opstart", "reqstart", "reqfail", "opfail" }, raisedEvents);
        }
    }
}
