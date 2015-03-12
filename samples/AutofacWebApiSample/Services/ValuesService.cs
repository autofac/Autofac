using System.Collections.Generic;

namespace AutofacWebApiSample.Services
{
    public class ValuesService : IValuesService
    {
        readonly ILogger _logger;

        public ValuesService(ILogger logger)
        {
            _logger = logger;
        }

        public IEnumerable<string> FindAll()
        {
            _logger.Log("FindAll called");

            return new[] {"value1", "value2"};
        }

        public string Find(int id)
        {
            _logger.Log("Find called with {0}", id);

            return string.Format("value{0}", id);
        }
    }
}