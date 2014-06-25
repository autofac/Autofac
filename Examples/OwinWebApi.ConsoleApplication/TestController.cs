using System.Web.Http;

namespace OwinWebApi.ConsoleApplication
{
    public class TestController : ApiController
    {
        readonly ILogger _logger;

        public TestController(ILogger logger)
        {
            _logger = logger;
        }

        public string Get()
        {
            _logger.Write("Inside the 'Get' method of the '{0}' controller.", GetType().Name);

            return "Hello, world!";
        }
    }
}
