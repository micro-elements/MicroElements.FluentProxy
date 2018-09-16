using System.Threading;
using System.Threading.Tasks;

namespace MicroElements.FluentProxy
{
    public interface IFluentProxyFactory
    {
        Task<FluentProxyServer> CreateServer(IFluentProxySettings settings, CancellationToken cancellationToken = default);
    }
}
