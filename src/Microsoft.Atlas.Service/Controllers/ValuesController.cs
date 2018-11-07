using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Atlas.Service.Core.ContainerInstances;

namespace Microsoft.Atlas.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IContainerInstanceManager _containerInstanceManager;

        public ValuesController(IContainerInstanceManager containerInstanceManager)
        {
            _containerInstanceManager = containerInstanceManager;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CreateInstanceContext>> Get(string id)
        {
            var context = new CreateInstanceContext(
                instanceName: id,
                atlasVersion: "0.1.3730778",
                arguments: new[] { "--help" });

            await _containerInstanceManager.CreateInstance(context);

            return context;
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(string id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(string id)
        {
        }
    }
}
