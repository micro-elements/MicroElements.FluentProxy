using System.Threading;
using System.Threading.Tasks;

namespace MicroElements.FluentProxy
{
    /// <summary>
    /// Proxy factory.
    /// </summary>
    public interface IFluentProxyFactory
    {
        /// <summary>
        /// Creates and starts proxy server.
        /// </summary>
        /// <param name="settings">Settings.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task representing asynchronous operation.</returns>
        Task<FluentProxyServer> CreateServer(IFluentProxySettings settings, CancellationToken cancellationToken = default);
    }
}
