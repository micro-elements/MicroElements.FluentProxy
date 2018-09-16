using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace MicroElements.FluentProxy
{
    public class FluentProxyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<FluentProxyMiddleware> _logger;

        public FluentProxyMiddleware(RequestDelegate next, IHttpClientFactory httpClientFactory, ILogger<FluentProxyMiddleware> logger)
        {
            _next = next;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [UsedImplicitly]
        public Task InvokeAsync(HttpContext context, IFluentProxySettings settings)
        {
            return ProcessRequest(context, settings);
        }

        private async Task ProcessRequest(HttpContext httpContext, IFluentProxySettings settings)
        {
            HttpClient httpClient = settings.CreateHttpClient != null ? settings.CreateHttpClient(settings) : CreateHttpClient(settings);

            if (settings.InitializeHttpClient != null)
                httpClient = settings.InitializeHttpClient(httpClient, settings);

            string requestUri = httpContext.Request.GetEncodedPathAndQuery();

            // Create http request
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(new HttpMethod(httpContext.Request.Method), requestUri);

            // Copy headers to request
            foreach (var requestHeader in httpContext.Request.Headers)
            {
                if (requestHeader.Key == "Host")
                {
                    // localhost leads to SSL error.
                    continue;
                }

                httpRequestMessage.Headers.TryAddWithoutValidation(requestHeader.Key, requestHeader.Value.ToArray());
            }

            // Copy body to request
            if (httpContext.Request.ContentLength.HasValue)
            {
                httpRequestMessage.Content = new StreamContent(httpContext.Request.Body);
            }

            var logMessage = new FluentProxyLogMessage
            {
                RequestTime = DateTime.Now,
                RequestUrl = requestUri,
                RequestHeaders = httpContext.Request.Headers,
                RequestContent = null
            };

            try
            {
                settings.OnRequestStarted?.Invoke(logMessage);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "IFluentProxySettings.OnRequestStarted error.");
            }

            string responseText = null;
            if (settings.MockedResponse != null)
            {
                responseText = settings.MockedResponse(requestUri);
                logMessage.ResponseTime = DateTime.Now;
                logMessage.ResponseContent = responseText;
            }

            if (responseText == null)
            {
                // Invoke real http request
                HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
                responseText = await httpResponseMessage.Content.ReadAsStringAsync();
                logMessage.ResponseTime = DateTime.Now;
                logMessage.ResponseContent = responseText;

                // Copy headers to response
                foreach (var responseHeader in httpResponseMessage.Headers)
                {
                    httpContext.Response.Headers.Add(responseHeader.Key, new StringValues(responseHeader.Value.ToArray()));
                }

                logMessage.ResponseHeaders = httpContext.Response.Headers;

                httpContext.Response.StatusCode = (int)httpResponseMessage.StatusCode;
                httpContext.Response.ContentType = httpResponseMessage.Content.Headers.ContentType.ToString();

                logMessage.StatusCode = (int)httpResponseMessage.StatusCode;
            }

            try
            {
                settings.OnRequestFinished?.Invoke(logMessage);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "IFluentProxySettings.OnRequestFinished error.");
            }

            await httpContext.Response.WriteAsync(responseText);
        }

        private HttpClient CreateHttpClient(IFluentProxySettings settings)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient(settings.ExternalUrl);

            httpClient.BaseAddress = new Uri(settings.ExternalUrl);
            if (settings.Timeout.HasValue)
                httpClient.Timeout = settings.Timeout.Value;

            return httpClient;
        }
    }
}
