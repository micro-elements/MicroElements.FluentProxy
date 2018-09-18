using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace MicroElements.FluentProxy
{
    public class FluentProxySettings : IFluentProxySettings
    {
        public int InternalPort { get; set; }
        

        public string ExternalUrl { get; set; }
        public TimeSpan? Timeout { get; set; }

        public bool NoProxy { get; set; } = true;
        public bool CopyHeadersFromRequest { get; set; } = true;
        public bool CopyHeadersFromResponse { get; set; } = true;
        public string[] RequestHeadersNoCopy { get; set; }
        public string[] ResponseHeadersNoCopy { get; set; }

        public Func<IFluentProxySettings, HttpClient> CreateHttpClient { get; set; }
        public Func<HttpClient, IFluentProxySettings, HttpClient> InitializeHttpClient { get; set; }
        public ILogger Logger { get; set; }

        public Action<FluentProxyLogMessage> OnRequestStarted { get; set; }
        public Action<FluentProxyLogMessage> OnRequestFinished { get; set; }

        public Func<FluentProxyLogMessage, FluentProxyLogMessage> GetMockedResponse { get; set; }

        public FluentProxySettings()
        {
        }

        //todo: test
        public FluentProxySettings(IFluentProxySettings settings)
        {
            InternalPort = settings.InternalPort;
            ExternalUrl = settings.ExternalUrl;
            Timeout = settings.Timeout;
            CreateHttpClient = settings.CreateHttpClient;
            InitializeHttpClient = settings.InitializeHttpClient;
            Logger = settings.Logger;
            OnRequestStarted = settings.OnRequestStarted;
            OnRequestFinished = settings.OnRequestFinished;
            GetMockedResponse = settings.GetMockedResponse;
        }
    }
}
