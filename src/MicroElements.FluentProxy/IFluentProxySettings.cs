using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace MicroElements.FluentProxy
{
    /// <summary>
    /// FluentProxySettings uses for customize access to one external service.
    /// </summary>
    public interface IFluentProxySettings
    {
        /// <summary>
        /// Gets the URL of external http service.
        /// </summary>
        Uri ExternalUrl { get; }

        /// <summary>
        /// Gets timeout for external service.
        /// </summary>
        TimeSpan? Timeout { get; }

        /// <summary>
        /// Gets the internal port.
        /// </summary>
        int InternalPort { get; }

        /// <summary>
        /// Gets the URL of the proxy http service.
        /// </summary>
        Uri ProxyUrl { get; }

        /// <summary>
        /// Copy headers from request to real request.
        /// </summary>
        bool CopyHeadersFromRequest { get; }

        /// <summary>
        /// Copy headers from response to result response.
        /// </summary>
        bool CopyHeadersFromResponse { get; }

        /// <summary>
        /// Request headers that shouldn't be copied to request.
        /// </summary>
        string[] RequestHeadersNoCopy { get; }

        /// <summary>
        /// Response headers that shouldn't be copied to result response.
        /// </summary>
        string[] ResponseHeadersNoCopy { get; }

        /// <summary>
        /// Gets request url.
        /// </summary>
        Func<IFluentProxySettings, HttpRequest, Uri> GetRequestUrl { get; }

        /// <summary>
        /// Factory function for creating HttpClient.
        /// </summary>
        Func<IFluentProxySettings, HttpClient> CreateHttpClient { get; }

        /// <summary>
        /// Initialize HttpClient function.
        /// </summary>
        Func<HttpClient, IFluentProxySettings, HttpClient> InitializeHttpClient { get; }

        /// <summary>
        /// Action fired after request started.
        /// </summary>
        Action<RequestSession> OnRequestStarted { get; set; }

        /// <summary>
        /// Action fired after request finished and response got.
        /// </summary>
        Action<RequestSession> OnRequestFinished { get; set; }

        /// <summary>
        /// Extension point to retrieve cached response.
        /// Can be used for tests, mocks and cache.
        /// </summary>
        Func<RequestSession, ResponseData> GetCachedResponse { get; set; }
    }
}
