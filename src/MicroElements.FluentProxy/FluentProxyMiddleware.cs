using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;

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

            if (settings.Timeout.HasValue)
                httpClient.Timeout = settings.Timeout.Value;

            if (settings.InitializeHttpClient != null)
                httpClient = settings.InitializeHttpClient(httpClient, settings);

            HttpRequest httpRequest = httpContext.Request;
            HttpResponse httpResponse = httpContext.Response;

            // Get full external Url
            Uri externalUriFull;
            string requestPathAndQuery = httpRequest.GetEncodedPathAndQuery();
            if (settings.GetRequestUrl != null)
            {
                // todo: use its value?
                externalUriFull = settings.GetRequestUrl(settings, httpRequest);
            }
            else
            {
                externalUriFull = new Uri(settings.ExternalUrl, requestPathAndQuery);
            }

            var session = new RequestSession
            {
                RequestId = Guid.NewGuid().ToString(), // httpContext.TraceIdentifier? // todo external ID
                RequestTime = DateTime.Now,
                RequestUrl = externalUriFull,
                RequestHeaders = httpRequest.Headers.ToDictionary(pair => pair.Key, pair => pair.Value.ToString()),
                RequestContent = null,//todo: read and rewind content if set in settings
            };

            // todo: to docs
            httpContext.Items["FluentProxySession"] = session;

            // Create http request
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(new HttpMethod(httpRequest.Method), requestPathAndQuery);

            // Fill request headers
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

            httpRequestMessage.Headers.Host = settings.ExternalUrl.Authority;

            // Fill request body
            if (httpRequest.ContentLength.HasValue)
            {
                httpRequestMessage.Content = new StreamContent(httpRequest.Body);
            }

            try
            {
                settings.OnRequestStarted?.Invoke(session);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "IFluentProxySettings.OnRequestStarted error.");
            }

            try
            {
                // Try get response from cache
                ResponseData responseData = settings.GetCachedResponse?.Invoke(session);
                if (responseData != null && responseData.IsOk)
                {
                    session.ResponseData = responseData;
                    session.ResponseSource = ResponseSource.Cache;

                    httpResponse.StatusCode = responseData.StatusCode;

                    // Fill response headers
                    if (settings.CopyHeadersFromResponse)
                    {
                        foreach (var responseHeader in responseData.ResponseHeaders)
                        {
                            if (settings.ResponseHeadersNoCopy != null && settings.ResponseHeadersNoCopy.Contains(responseHeader.Key, StringComparer.InvariantCultureIgnoreCase))
                                continue;

                            httpResponse.Headers[responseHeader.Key] = responseHeader.Value;
                        }
                    }

                    // SendAsync removes chunking from the response. This removes the header so it doesn't expect a chunked response.
                    httpResponse.Headers.Remove("transfer-encoding");
                }
                else
                {
                    // Invoke real http request
                    HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

                    responseData = new ResponseData
                    {
                        RequestId = session.RequestId,
                        ResponseId = Guid.NewGuid().ToString(),
                        StatusCode = (int)httpResponseMessage.StatusCode,
                    };

                    httpResponse.StatusCode = responseData.StatusCode;

                    // Fill response headers
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

                    // Fill headers
                    responseData.ResponseHeaders = httpResponse.Headers.ToDictionary(pair => pair.Key, pair => pair.Value.ToString());

                    // Read content
                    string responseText = await httpResponseMessage.Content.ReadAsStringAsync();
                    responseData.ResponseTime = DateTime.Now;
                    responseData.ResponseContent = responseText;
                    session.ResponseData = responseData;
                    session.ResponseSource = ResponseSource.HttpResponse;
                }

                // Write response content
                if (session.ResponseData.ResponseContent != null)
                    await httpResponse.WriteAsync(session.ResponseData.ResponseContent);

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in request processing.");
                httpResponse.StatusCode = (int)HttpStatusCode.InternalServerError;

                if (session.ResponseData == null)
                    session.ResponseData = new ResponseData();
                session.ResponseData.Exception = e;
                session.ResponseData.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            finally
            {
                try
                {
                    settings.OnRequestFinished?.Invoke(session);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "IFluentProxySettings.OnRequestFinished error.");
                }
            }
        }

        private HttpClient CreateHttpClient(IFluentProxySettings settings)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient(settings.ExternalUrl.Authority);

            httpClient.BaseAddress = settings.ExternalUrl;

            return httpClient;
        }
    }
}
