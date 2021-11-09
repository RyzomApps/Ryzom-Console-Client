using Client;
using Xunit;

namespace Tests
{
    public class ClientTest
    {
        [Fact]
        public void ClientInstanceTest()
        {
            var client = new RyzomClient(false);

            Assert.True(client != null);
        }
    }
}
