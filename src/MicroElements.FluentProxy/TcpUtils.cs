using System.Net;
using System.Net.Sockets;

namespace MicroElements.FluentProxy
{
    public static class TcpUtils
    {
        public static int FindFreeTcpPort()
        {
            TcpListener tcpListener = null;
            try
            {
                tcpListener = new TcpListener(IPAddress.Loopback, 0);
                tcpListener.Start();

                return ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            }
            finally
            {
                tcpListener?.Stop();
            }
        }
    }
}
