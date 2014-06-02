using System.Security;
using System.Threading.Tasks;
using Autofac.Integration.Owin;
using Microsoft.Owin;

namespace Autofac.Tests.Integration.Owin
{
    public class TestMiddleware : OwinMiddleware
    {
        public TestMiddleware(OwinMiddleware next)
            : base(next)
        {
            LifetimeScope = null;
        }

        public static ILifetimeScope LifetimeScope { get; set; }

        [SecurityCritical]
        public override Task Invoke(IOwinContext context)
        {
            LifetimeScope = context.GetAutofacLifetimeScope();
            return Next.Invoke(context);
        }
    }
}
