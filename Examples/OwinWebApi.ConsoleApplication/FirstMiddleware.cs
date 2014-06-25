using System.Threading.Tasks;
using Microsoft.Owin;

namespace OwinWebApi.ConsoleApplication
{
    public class FirstMiddleware : OwinMiddleware
    {
        private readonly ILogger _logger;

        public FirstMiddleware(OwinMiddleware next, ILogger logger) : base(next)
        {
            _logger = logger;
        }

        public override async Task Invoke(IOwinContext context)
        {
            _logger.Write("Inside the 'Invoke' method of the '{0}' middleware.", GetType().Name);

            await Next.Invoke(context);
        }
    }
}
