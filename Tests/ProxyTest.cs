using Xunit;
using Client.Network.Proxy;

namespace Tests
{
    public class ProxyTest
    {
        [Fact]
        public void Socks5Test()
        {
            Socks5Proxy.EstablishConnection("79.143.187.168", 7497, "164.132.202.87", 53, "", "", out var udpAdress, out var udpPort);

            Assert.NotNull(udpAdress);

            Assert.Equal("\0", udpAdress);

            Assert.Equal(7497, udpPort);
        }
    }
}
