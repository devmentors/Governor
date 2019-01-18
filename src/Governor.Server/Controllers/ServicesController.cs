using System.Collections.Generic;
using Governor.Server.Managers;
using Governor.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Governor.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly ServiceManager _manager;

        public ServicesController(ServiceManager manager)
            => _manager = manager;

        [HttpGet]
        public ActionResult<IEnumerable<ServiceInfo>> GetServices()
            => Ok(_manager.GetServices());
        
        [HttpPost("{name}/start")]
        public ActionResult Start(string name)
        {
            _manager.Start(name);
            return Ok();
        }

        [HttpPost("{name}/kill")]
        public ActionResult Kill(string name)
        {
            _manager.Kill(name);
            return Ok();
        }
    }
}