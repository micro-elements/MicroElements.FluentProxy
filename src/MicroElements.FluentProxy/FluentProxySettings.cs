using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace MicroElements.FluentProxy
{
    /// <summary>
    /// FluentProxySettings uses for customize access to one external service.
    /// </summary>
    public class FluentProxySettings : IFluentProxySettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FluentProxySettings"/> class.
        /// </summary>
        public FluentProxySettings()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentProxySettings"/> class.
        /// </summary>
        public FluentProxySettings(IFluentProxySettings settings)
        {
            InternalPort = settings.InternalPort;
            ExternalUrl = settings.ExternalUrl;
            ProxyUrl = settings.ProxyUrl;
            Timeout = settings.Timeout;
            CreateHttpClient = settings.CreateHttpClient;
            InitializeHttpClient = settings.InitializeHttpClient;
            OnRequestStarted = settings.OnRequestStarted;
            OnRequestFinished = settings.OnRequestFinished;
            GetCachedResponse = settings.GetCachedResponse;
            CopyHeadersFromRequest = settings.CopyHeadersFromRequest;
            CopyHeadersFromResponse = settings.CopyHeadersFromResponse;
            RequestHeadersNoCopy = settings.RequestHeadersNoCopy;
            ResponseHeadersNoCopy = settings.ResponseHeadersNoCopy;
        }

        /// <inheritdoc />
        public Uri ExternalUrl { get; set; }

        /// <inheritdoc />
        public TimeSpan? Timeout { get; set; }

        /// <inheritdoc />
        public int InternalPort { get; set; }

        /// <inheritdoc />
        public Uri ProxyUrl { get; set; }

        /// <inheritdoc />
        public bool CopyHeadersFromRequest { get; set; } = true;

        /// <inheritdoc />
        public bool CopyHeadersFromResponse { get; set; } = true;

        /// <inheritdoc />
        public string[] RequestHeadersNoCopy { get; set; }

        /// <inheritdoc />
        public string[] ResponseHeadersNoCopy { get; set; }

        /// <inheritdoc />
        public Func<IFluentProxySettings, HttpRequest, Uri> GetRequestUrl { get; set; }

        /// <inheritdoc />
        public Func<IFluentProxySettings, HttpClient> CreateHttpClient { get; set; }

        /// <inheritdoc />
        public Func<HttpClient, IFluentProxySettings, HttpClient> InitializeHttpClient { get; set; }

        /// <inheritdoc />
        public Action<RequestSession> OnRequestStarted { get; set; }

        /// <inheritdoc />
        public Action<RequestSession> OnRequestFinished { get; set; }

        /// <inheritdoc />
        public Func<RequestSession, ResponseData> GetCachedResponse { get; set; }
    }
}
