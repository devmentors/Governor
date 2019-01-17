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

        public IEnumerable<ServiceInfo> GetServices()
            => _manager.GetServices();
        
        [HttpPost("{name}/start")]
        public void Start(string name)
            => _manager.Start(name);

        [HttpPost("{name}/kill")]
        public void Kill(string name)
            => _manager.Kill(name);
    }
}