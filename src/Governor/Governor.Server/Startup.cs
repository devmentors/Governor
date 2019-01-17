using Microsoft.AspNetCore.Blazor.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Net.Mime;
using Governor.Server.Builders;
using Governor.Server.Domain;
using Governor.Server.Managers;
using Governor.Server.Options;
using Microsoft.Extensions.Configuration;

namespace Governor.Server
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
            => Configuration = configuration;
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<ServicesOptions>(Configuration.GetSection("Services"));
            services.AddSingleton<ServiceBuilder, ServiceBuilder>();
            services.AddSingleton<ServiceManager, ServiceManager>();
            
            services.AddMvc();

            services.AddResponseCompression(options =>
            {
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                {
                    MediaTypeNames.Application.Octet,
                    WasmMediaTypeNames.Application.Wasm,
                });
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, 
            ServiceManager manager, IApplicationLifetime applicationLifetime)
        {
            app.UseResponseCompression();

            manager.Init();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "default", template: "{controller}/{action}/{id?}");
            });
            
            applicationLifetime.ApplicationStopped.Register(manager.KillAll);

            app.UseBlazor<Client.Startup>();
        }
    }
}
