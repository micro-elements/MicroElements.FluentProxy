using System;
using System.Collections.Generic;
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

            HttpRequest httpRequest = httpContext.Request;
            HttpResponse httpResponse = httpContext.Response;

            // Get result uri //todo: callback
            string requestUri = httpRequest.GetEncodedPathAndQuery();
            Uri externalUri = new Uri(settings.ExternalUrl);
            Uri externalUriFull = new Uri(externalUri, requestUri);

            // Create http request
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(new HttpMethod(httpRequest.Method), requestUri);

            // Copy headers to request
            if (settings.CopyHeadersFromRequest)
            {
                foreach (var requestHeader in httpRequest.Headers)
                {
                    if (settings.RequestHeadersNoCopy != null && settings.RequestHeadersNoCopy.Contains(requestHeader.Key, StringComparer.InvariantCultureIgnoreCase))
                        continue;
                    if (!httpRequestMessage.Headers.TryAddWithoutValidation(requestHeader.Key, requestHeader.Value.ToArray()) && httpRequestMessage.Content != null)
                    {
                        httpRequestMessage.Content?.Headers.TryAddWithoutValidation(requestHeader.Key, requestHeader.Value.ToArray());
                    }
                }
            }

            httpRequestMessage.Headers.Host = externalUri.Authority;

            // Copy body to request
            if (httpRequest.ContentLength.HasValue)
            {
                httpRequestMessage.Content = new StreamContent(httpRequest.Body);
            }

            var logMessage = new FluentProxyLogMessage
            {
                RequestTime = DateTime.Now,
                RequestUrl = externalUriFull.ToString(),
                RequestHeaders = httpRequest.Headers,
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

                httpResponse.StatusCode = (int)httpResponseMessage.StatusCode;
                logMessage.StatusCode = (int)httpResponseMessage.StatusCode;

                // Copy headers to response
                if (settings.CopyHeadersFromResponse)
                {
                    foreach (var responseHeader in httpResponseMessage.Headers)
                    {
                        if (settings.ResponseHeadersNoCopy != null && settings.ResponseHeadersNoCopy.Contains(responseHeader.Key, StringComparer.InvariantCultureIgnoreCase))
                            continue;

                        httpResponse.Headers[responseHeader.Key] = responseHeader.Value.ToArray();
                    }

                    foreach (var responseHeader in httpResponseMessage.Content.Headers)
                    {
                        if (settings.ResponseHeadersNoCopy != null && settings.ResponseHeadersNoCopy.Contains(responseHeader.Key, StringComparer.InvariantCultureIgnoreCase))
                            continue;

                        httpResponse.Headers[responseHeader.Key] = responseHeader.Value.ToArray();
                    }
                }

                // SendAsync removes chunking from the response. This removes the header so it doesn't expect a chunked response.
                httpResponse.Headers.Remove("transfer-encoding");

                logMessage.ResponseHeaders = httpResponse.Headers;
            }

            try
            {
                settings.OnRequestFinished?.Invoke(logMessage);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "IFluentProxySettings.OnRequestFinished error.");
            }

            if (responseText != null)
                await httpResponse.WriteAsync(responseText);
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
