using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Hosting;
using Autofac.Integration.Owin;

namespace Autofac.Integration.WebApi.Owin
{
    [SecurityCritical]
    class DependencyScopeHandler : DelegatingHandler
    {
        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException("request");

            var owinContext = request.GetOwinContext();
            if (owinContext == null) return base.SendAsync(request, cancellationToken);

            var lifetimeScope = owinContext.GetAutofacLifetimeScope();
            if (lifetimeScope == null) return base.SendAsync(request, cancellationToken);

            var dependencyScope = new AutofacWebApiDependencyScope(lifetimeScope);
            request.Properties[HttpPropertyKeys.DependencyScope] = dependencyScope;
            request.RegisterForDispose(dependencyScope);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
