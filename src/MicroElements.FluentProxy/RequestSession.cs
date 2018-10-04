using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace MicroElements.FluentProxy
{
    /// <summary>
    /// Detailed info about one request-response session.
    /// </summary>
    /// todo: comments
    public class RequestSession
    {
        public string RequestId { get; set; }
        public DateTime RequestTime { get; set; }
        public Uri RequestUrl { get; set; }
        public string RequestContent { get; set; }
        //todo: to dic<string,string>
        public IDictionary<string, string> RequestHeaders { get; set; }

        public ResponseData ResponseData { get; set; }
        public ResponseSource ResponseSource { get; set; }

        public TimeSpan Duration => ResponseData.ResponseTime - RequestTime;
        public bool IsOk => ResponseData?.IsOk ?? false;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(RequestTime)}: {RequestTime:s}, {nameof(RequestUrl)}: {RequestUrl}, {nameof(ResponseData.StatusCode)}: {ResponseData.StatusCode}";
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
        public IDictionary<string, string> ResponseHeaders { get; set; }

        public bool IsOk => StatusCode == 200 && Exception == null;
    }
}
