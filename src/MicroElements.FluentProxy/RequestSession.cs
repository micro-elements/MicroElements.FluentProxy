using System;
using Microsoft.AspNetCore.Http;

namespace MicroElements.FluentProxy
{
    /// <summary>
    /// Detailed info about one request-response session.
    /// </summary>
    public class RequestSession
    {
        public string RequestId { get; set; }
        public DateTime RequestTime { get; set; }
        public Uri RequestUrl { get; set; }
        public string RequestContent { get; set; }
        public IHeaderDictionary RequestHeaders { get; set; }

        public ResponseData ResponseData { get; set; }
        public ResponseSource ResponseSource { get; set; }

        public DateTime ResponseTime => ResponseData.ResponseTime;
        public int StatusCode => ResponseData.StatusCode;
        public Exception Exception => ResponseData.Exception;
        public string ResponseContent => ResponseData.ResponseContent;
        public IHeaderDictionary ResponseHeaders => ResponseData.ResponseHeaders;

        public TimeSpan Duration => ResponseTime - RequestTime;
        public bool IsOk => ResponseData?.IsOk ?? false;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(RequestTime)}: {RequestTime:s}, {nameof(RequestUrl)}: {RequestUrl}, {nameof(StatusCode)}: {StatusCode}";
        }
    }

    public enum ResponseSource
    {
        HttpResponse,
        Cache,
    }

    public class ResponseData
    {
        public string RequestId { get; set; }
        public string ResponseId { get; set; }

        public DateTime ResponseTime { get; set; }
        public int StatusCode { get; set; }
        public Exception Exception { get; set; }
        public string ResponseContent { get; set; }
        public IHeaderDictionary ResponseHeaders { get; set; }

        public bool IsOk => StatusCode == 200 && Exception != null;
    }
}
