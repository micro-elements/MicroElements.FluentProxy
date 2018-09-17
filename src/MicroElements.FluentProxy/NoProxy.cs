using System;
using System.Net;

namespace MicroElements.FluentProxy
{
    /// <summary>
    /// NoProxy bypasses every url.
    /// </summary>
    public class NoProxy : IWebProxy
    {
        public static readonly NoProxy Instance = new NoProxy();

        private NoProxy() { }

        /// <inheritdoc />
        public Uri GetProxy(Uri destination) => throw new NotImplementedException();

        /// <inheritdoc />
        public bool IsBypassed(Uri host) => true;

        /// <inheritdoc />
        public ICredentials Credentials { get; set; }
    }
}
