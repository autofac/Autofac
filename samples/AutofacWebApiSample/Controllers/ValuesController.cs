using System.Collections.Generic;
using AutofacWebApiSample.Services;
using Microsoft.AspNetCore.Mvc;

namespace AutofacWebApiSample.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        readonly IValuesService _valuesService;

        public ValuesController(IValuesService valuesService)
        {
            _valuesService = valuesService;
        }
        
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return _valuesService.FindAll();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return _valuesService.Find(id);
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
