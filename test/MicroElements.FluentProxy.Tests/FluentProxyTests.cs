using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MicroElements.FluentProxy.Tests
{
    public class FluentProxyTests
    {
        static readonly FluentProxyFactory FluentProxyFactory = new FluentProxyFactory();

        [Fact]
        public async Task CreateServerWithSameUrlShouldReturnSameValues()
        {
            var settings = new FluentProxySettings
            {
                InternalPort = 5000,
                ExternalUrl = new Uri("https://api.exmo.com"),
            };
            var fluentProxyServer1 = await FluentProxyFactory.CreateServer(settings);
            var fluentProxyServer2 = await FluentProxyFactory.CreateServer(settings);

            fluentProxyServer1.Should().BeSameAs(fluentProxyServer2);
        }

        [Fact]
        public async Task CreateServerWithSameUrlShouldReturnSameValues2()
        {
            var settings = new FluentProxySettings
            {
                ExternalUrl = new Uri("https://api.exmo.com"),
            };
            var fluentProxyServer1 = await FluentProxyFactory.CreateServer(settings);
            var fluentProxyServer2 = await FluentProxyFactory.CreateServer(settings);

            fluentProxyServer1.Should().BeSameAs(fluentProxyServer2);
        }

        [Fact]
        public async Task CreateServerWithSameUrlButDifferentPortShouldNotBeSame()
        {
            var settings1 = new FluentProxySettings
            {
                InternalPort = TcpUtils.FindFreeTcpPort(),
                ExternalUrl = new Uri("https://api.exmo.com"),
            };

            var settings2 = new FluentProxySettings
            {
                InternalPort = TcpUtils.FindFreeTcpPort(),
                ExternalUrl = new Uri("https://api.exmo.com"),
            };

            settings1.InternalPort.Should().NotBe(settings2.InternalPort);

            var fluentProxyServer1 = await FluentProxyFactory.CreateServer(settings1);
            var fluentProxyServer2 = await FluentProxyFactory.CreateServer(settings2);

            fluentProxyServer1.Should().NotBeSameAs(fluentProxyServer2);
        }

        [Fact]
        public async Task Stub()
        {
            RequestSession requestSession;
            var settings = new FluentProxySettings
            {
                InternalPort = 5000,
                ExternalUrl = new Uri("https://api.exmo.com/"),
                OnRequestFinished = message => requestSession = message
            };
            var fluentProxyServer = await FluentProxyFactory.CreateServer(settings);
            
            HttpClient httpClient = fluentProxyServer.GetHttpClient();
            string response = await httpClient.GetStringAsync("v1/currency/");
            response.Should().Contain("USD");
            
        }

        [Fact]
        public async Task Stub3()
        {
            var settings = new FluentProxySettings
            {
                InternalPort = 5000,
                ExternalUrl = new Uri("https://api.exmo.com")
            };
            var fluentProxyServer = await FluentProxyFactory.CreateServer(settings);
            HttpClient httpClient = fluentProxyServer.GetHttpClient();
            string response = await httpClient.GetStringAsync("v1/currency?title=aa&action=edit");
            response.Should().Contain("USD");
        }

        [Fact]
        public async Task GithubBranches()
        {
            var settings = new FluentProxySettings
            {
                ExternalUrl = new Uri("https://api.github.com"),
                OnRequestFinished = session =>
                {
                    Console.WriteLine(session.RequestUrl);
                    Console.WriteLine(session.ResponseData.ResponseContent);
                }
            };
            FluentProxyServer fluentProxyServer = await FluentProxyFactory.CreateServer(settings);

            var request = new HttpRequestMessage(HttpMethod.Get, "/repos/aspnet/docs/branches");
            request.Headers.Add("Accept", "application/vnd.github.v3+json");
            request.Headers.Add("User-Agent", "HttpClientFactory-Sample");

            HttpClient httpClient = fluentProxyServer.GetHttpClient();

            var httpResponseMessage = await httpClient.SendAsync(request);
            var response = await httpResponseMessage.Content.ReadAsStringAsync();

            httpResponseMessage.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task GithubBranchesAspNet()
        {
            var settings = new FluentProxySettings
            {
                ExternalUrl = new Uri("https://api.github.com/repos/aspnet/"),
            };
            FluentProxyServer fluentProxyServer = await FluentProxyFactory.CreateServer(settings);

            var request = new HttpRequestMessage(HttpMethod.Get, "docs/branches");
            request.Headers.Add("Accept", "application/vnd.github.v3+json");
            request.Headers.Add("User-Agent", "HttpClientFactory-Sample");

            HttpClient httpClient = fluentProxyServer.GetHttpClient();

            var httpResponseMessage = await httpClient.SendAsync(request);
            var response = await httpResponseMessage.Content.ReadAsStringAsync();

            httpResponseMessage.EnsureSuccessStatusCode();
        }

        [Fact]
        public void FluentProxySettingsCopyShouldBeEqualToInitialSettings()
        {
            FluentProxySettings settings = new FluentProxySettings
            {
                InternalPort = 42,
                ExternalUrl = new Uri("https://api.github.com"),
                ProxyUrl = new Uri("http://localhost:8080/resource"),
                CreateHttpClient = proxySettings => new HttpClient(),
                Timeout = TimeSpan.FromSeconds(1),
                OnRequestFinished = message => { },
                OnRequestStarted = message => { },
                InitializeHttpClient = (client, proxySettings) => client
            };

            FluentProxySettings settingsCopy = new FluentProxySettings(settings);
            foreach (var propertyInfo in settingsCopy.GetType().GetProperties())
            {
                propertyInfo.GetValue(settingsCopy).Should().BeEquivalentTo(propertyInfo.GetValue(settings));
            }
        }
    }
}
