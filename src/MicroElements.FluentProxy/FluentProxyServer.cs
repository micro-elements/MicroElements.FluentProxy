using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;

namespace MicroElements.FluentProxy
{
    /// <summary>
    /// Proxy server.
    /// Contains <see cref="IFluentProxySettings"/> and reference to <see cref="IWebHost"/>.
    /// <para>On dispose stops WebHost and fires onDispose event.</para>
    /// </summary>
    public class FluentProxyServer : IDisposable
    {
        private readonly Action _onDispose;
        private readonly Lazy<HttpClient> _internalHttpClient;

        public IFluentProxySettings Settings { get; }
        public IWebHost WebHost { get; }

        public FluentProxyServer(IFluentProxySettings settings, IWebHost webHost, Action onDispose)
        {
            _onDispose = onDispose;
            Settings = settings;
            WebHost = webHost;
            _internalHttpClient = new Lazy<HttpClient>(() => new HttpClient
            {
                BaseAddress = settings.ProxyUrl,
            });
        }

        /// <inheritdoc />
        public void Dispose()
        {
            try
            {
                WebHost.StopAsync().GetAwaiter().GetResult();
                WebHost?.Dispose();
                _onDispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Gets proxied http client.
        /// </summary>
        /// <returns><see cref="HttpClient"/>.</returns>
        public HttpClient GetHttpClient() => _internalHttpClient.Value;
    }
}
