using System;
using Microsoft.AspNetCore.Http;

namespace MicroElements.FluentProxy
{
    /// <summary>
    /// Detailed info about one request-response session.
    /// </summary>
    public class FluentProxyLogMessage
    {
        public DateTime RequestTime;
        public Uri RequestUrl;
        public string RequestContent;
        public IHeaderDictionary RequestHeaders;

        public DateTime ResponseTime;
        public int StatusCode;
        public Exception Exception;
        public string ResponseContent;
        public IHeaderDictionary ResponseHeaders;

        public TimeSpan Duration => ResponseTime - RequestTime;

        public bool IsOk => StatusCode == 200 && Exception != null;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(RequestTime)}: {RequestTime:s}, {nameof(RequestUrl)}: {RequestUrl}, {nameof(StatusCode)}: {StatusCode}";
        }
    }
}
