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
                ExternalUrl = "https://api.exmo.com",
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
                ExternalUrl = "https://api.exmo.com",
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
                ExternalUrl = "https://api.exmo.com",
            };

            var settings2 = new FluentProxySettings
            {
                InternalPort = TcpUtils.FindFreeTcpPort(),
                ExternalUrl = "https://api.exmo.com",
            };

            settings1.InternalPort.Should().NotBe(settings2.InternalPort);

            var fluentProxyServer1 = await FluentProxyFactory.CreateServer(settings1);
            var fluentProxyServer2 = await FluentProxyFactory.CreateServer(settings2);

            fluentProxyServer1.Should().NotBeSameAs(fluentProxyServer2);
        }

        [Fact]
        public async Task Stub()
        {
            FluentProxyLogMessage fluentProxyLogMessage;
            var settings = new FluentProxySettings
            {
                InternalPort = 5000,
                ExternalUrl = "https://api.exmo.com/",
                OnRequestFinished = message => fluentProxyLogMessage = message
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
                ExternalUrl = "https://api.exmo.com"
            };
            var fluentProxyServer = await FluentProxyFactory.CreateServer(settings);
            HttpClient httpClient = fluentProxyServer.GetHttpClient();
            string response = await httpClient.GetStringAsync("v1/currency?title=aa&action=edit");
            response.Should().Contain("USD");
        }

        [Fact]
        public async Task Stub2()
        {
            var settings = new FluentProxySettings
            {
                InternalPort = 5001,
                ExternalUrl = "https://api.github.com",
            };
            FluentProxyServer fluentProxyServer = await FluentProxyFactory.CreateServer(settings);

            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/aspnet/docs/branches");
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
                ExternalUrl = "ExternalUrl",
                CreateHttpClient = proxySettings => new HttpClient(),
                Logger = NullLogger.Instance,
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
