// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace MicroElements.FluentProxy
{
    public class FluentProxyFactory : IFluentProxyFactory
    {
        private readonly ConcurrentDictionary<string, Task<FluentProxyServer>> _webHosts = new ConcurrentDictionary<string, Task<FluentProxyServer>>();

        public Task<FluentProxyServer> CreateServer(IFluentProxySettings settings, CancellationToken cancellationToken = default)
        {
            string serverKey = $"{settings.ExternalUrl};{settings.InternalPort}";
            return _webHosts.GetOrAdd(serverKey, url => CreateServerInternal(settings, cancellationToken));
        }

        private async Task<FluentProxyServer> CreateServerInternal(IFluentProxySettings settings, CancellationToken cancellationToken)
        {
            if (settings.InternalPort <= 0)
            {
                settings = new FluentProxySettings(settings) { InternalPort = TcpUtils.FindFreeTcpPort() };
            }
            if (settings.Logger == null)
            {
                settings = new FluentProxySettings(settings) { Logger = NullLogger.Instance };
            }

            IWebHost webHost = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"http://localhost:{settings.InternalPort}")
                .ConfigureServices(services => services.AddSingleton<IFluentProxySettings>(settings))
                .UseStartup<FluentProxyStartup>()
                .Build();
            await webHost.StartAsync(cancellationToken);
            string serverKey = $"{settings.ExternalUrl};{settings.InternalPort}";
            return new FluentProxyServer(settings, webHost, () => _webHosts.TryRemove(serverKey, out _));
        }
    }
}
