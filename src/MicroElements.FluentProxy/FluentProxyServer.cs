using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;

namespace MicroElements.FluentProxy
{
    public class FluentProxyServer : IDisposable
    {
        private readonly Action _onDispose;
        public IFluentProxySettings Settings { get; }
        public IWebHost WebHost { get; }
        private readonly Lazy<HttpClient> _internalHttpClient;

        public FluentProxyServer(IFluentProxySettings settings, IWebHost webHost, Action onDispose)
        {
            _onDispose = onDispose;
            Settings = settings;
            WebHost = webHost;
            _internalHttpClient = new Lazy<HttpClient>(() => new HttpClient
            {
                BaseAddress = new Uri($"http://localhost:{Settings.InternalPort}"),
            });
        }

        public void Dispose()
        {
            WebHost.StopAsync().GetAwaiter().GetResult();
            WebHost?.Dispose();
            _onDispose();
        }

        public HttpClient GetHttpClient() => _internalHttpClient.Value;
    }
}
