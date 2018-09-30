// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace MicroElements.FluentProxy
{
    /// <summary>
    /// Proxy factory.
    /// </summary>
    public class FluentProxyFactory : IFluentProxyFactory
    {
        private readonly ConcurrentDictionary<string, Task<FluentProxyServer>> _webHosts = new ConcurrentDictionary<string, Task<FluentProxyServer>>();

        /// <summary>
        /// Creates and starts proxy server.
        /// </summary>
        /// <param name="settings">Settings.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task representing asynchronous operation.</returns>
        public Task<FluentProxyServer> CreateServer(IFluentProxySettings settings, CancellationToken cancellationToken = default)
        {
            // Server key before port evaluation. That means that same url and undefined port returns same servers.
            string serverKey = ServerKey(settings);

            int internalPort = settings.InternalPort;
            if (internalPort <= 0)
                internalPort = TcpUtils.FindFreeTcpPort();

            // Recreate with port and proxyUrl
            settings = new FluentProxySettings(settings)
            {
                InternalPort = internalPort,
                ProxyUrl = new UriBuilder("http", "localhost", internalPort, settings.ExternalUrl.PathAndQuery).Uri,
            };

            return _webHosts.GetOrAdd(serverKey, url => CreateServerInternal(settings, serverKey, cancellationToken));
        }

        private async Task<FluentProxyServer> CreateServerInternal(IFluentProxySettings settings, string serverKey, CancellationToken cancellationToken)
        {
            IWebHost webHost = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"http://localhost:{settings.InternalPort}")
                .ConfigureServices(services => services.AddSingleton<IFluentProxySettings>(settings))
                .UseStartup<FluentProxyStartup>()
                .Build();
            await webHost.StartAsync(cancellationToken);

            return new FluentProxyServer(settings, webHost, () => _webHosts.TryRemove(serverKey, out _));
        }

        private string ServerKey(IFluentProxySettings settings) => $"{settings.ExternalUrl.AbsoluteUri};{settings.InternalPort}";
    }
}
