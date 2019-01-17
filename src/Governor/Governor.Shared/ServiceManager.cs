using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Governor.Shared
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
                    serviceOptions.FileName, serviceOptions.Arguments, serviceOptions.Url);
                _services.Add(service);
            }

            _initialized = true;
            _logger.LogInformation($"Initialized Service Manager. Services: " +
                                   $"{string.Join(Environment.NewLine, _services.Select(s => $"'{s.Name}', URL: '{s.Url}'"))}");
        }

        public void Start(string name)
        {
            _logger.LogInformation($"Starting a service: '{name}'.");
            GetServiceOrFail(name).Start();
            _logger.LogInformation($"Started a service: '{name}'.");
        }

        public void Kill(string name)
        {
            _logger.LogInformation($"Killing a service: '{name}'.");
            GetServiceOrFail(name).Kill();
            _logger.LogInformation($"Killed a service: '{name}'.");
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