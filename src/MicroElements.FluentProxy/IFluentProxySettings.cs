using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;

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
        string ExternalUrl { get; }

        /// <summary>
        /// Gets timeout for external service.
        /// </summary>
        TimeSpan? Timeout { get; }

        /// <summary>
        /// Gets the internal port.
        /// </summary>
        int InternalPort { get; }

        Func<IFluentProxySettings, HttpClient> CreateHttpClient { get; }
        Func<HttpClient, IFluentProxySettings, HttpClient> InitializeHttpClient { get; }
        ILogger Logger { get; }
        Action<FluentProxyLogMessage> OnRequestStarted { get; set; }
        Action<FluentProxyLogMessage> OnRequestFinished { get; set; }
        Func<string, string> MockedResponse { get; set; }
    }
}
