using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace MicroElements.FluentProxy
{
    /// <summary>
    /// Startup class for FluentProxy asp.net server.
    /// </summary>
    public class FluentProxyStartup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        [UsedImplicitly]
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            // services.AddHttpClient("FluentProxy").ConfigureHttpMessageHandlerBuilder(builder => builder.PrimaryHandler = null);
            // todo: ConfigureHttpMessageHandlerBuilder
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMiddleware<FluentProxyMiddleware>();
        }
    }
}
