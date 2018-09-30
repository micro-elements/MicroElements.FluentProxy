using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MicroElements.FluentProxy;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ProxyServiceCollectionExtensions
    {
        // todo
        internal static IServiceCollection AddProxy(this IServiceCollection services, IFluentProxySettings settings)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddSingleton<IFluentProxyFactory, FluentProxyFactory>();

            var server = new FluentProxyFactory().CreateServer(settings).Result;
         
            services.AddHttpClient<ProxiedHttpClient>(client =>
            {
                client.BaseAddress = settings.ExternalUrl;
                settings.InitializeHttpClient?.Invoke(client, settings);
            });

            return services;
        }
    }

    public class ProxiedHttpClient
    {
        public ProxiedHttpClient(HttpClient client)
        {
            Client = client;
        }

        public HttpClient Client { get; }
    }
}
