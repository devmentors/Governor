using System;
using System.Collections.Generic;
using System.Linq;
using Governor.Server.Builders;
using Governor.Server.Domain;
using Governor.Server.Options;
using Governor.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Governor.Server.Managers
{
    public class ServiceManager
    {
        private bool _initialized;
        private readonly ISet<Service> _services = new HashSet<Service>();
        private readonly ServiceBuilder _serviceBuilder;
        private readonly IOptions<ServicesOptions> _servicesOptions;
        private readonly ILogger<ServiceManager> _logger;

        public ServiceManager(ServiceBuilder serviceBuilder,
            IOptions<ServicesOptions> servicesOptions,
            ILogger<ServiceManager> logger)
        {
            _serviceBuilder = serviceBuilder;
            _servicesOptions = servicesOptions;
            _logger = logger;
        }

        public IEnumerable<ServiceInfo> GetServices() => _services.Select(s => new ServiceInfo
        {
            Name = s.Name,
            IsRunning = s.IsRunning,
            Url = s.Url
        });

        public void Init()
        {
            _logger.LogInformation("Initializing Service Manager...");
            if (_initialized)
            {
                throw new InvalidOperationException("Service manager was already initialized.");
            }

            foreach (var serviceOptions in _servicesOptions.Value)
            {
                var service = _serviceBuilder.Build(serviceOptions.Name, serviceOptions.Directory,
                    serviceOptions.FileName, serviceOptions.Arguments, serviceOptions.Url, serviceOptions.SharedShell);
                _services.Add(service);
            }

            _initialized = true;
            _logger.LogInformation($"Initialized Service Manager. Services:{Environment.NewLine}" +
                                   $"{string.Join(Environment.NewLine, _services.Select(s => $"'{s.Name}', URL: '{s.Url}'"))}");
        }

        public void Start(string name)
        {
            var service = GetServiceOrFail(name);
            _logger.LogInformation($"Starting a service: '{name}'.");
            service.Start();
            _logger.LogInformation($"Started a service: '{name}' [PID: {service.Id}].");
        }

        public void Kill(string name)
        {
            var service = GetServiceOrFail(name);
            var id = service.Id;
            _logger.LogInformation($"Killing a service: '{name}' [PID: {id}].");
            service.Kill();
            _logger.LogInformation($"Killed a service: '{name}' [PID: {id}].");
        }

        public void KillAll()
        {
            var servicesToKill = _services.Where(s => s.IsRunning).ToList();
            if (!servicesToKill.Any())
            {
                _logger.LogInformation($"No running services found to be killed.");

                return;
            }
            
            _logger.LogInformation("Killing all services.");
            servicesToKill.ForEach(s => s.Kill());
            _logger.LogInformation("Killed all services.");
        }

        private Service GetServiceOrFail(string name)
        {
            var service = _services.SingleOrDefault(s =>
                s.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (service is null)
            {
                throw new ArgumentException($"Service: '{name}' was not found.", nameof(name));
            }

            return service;
        }
    }
}