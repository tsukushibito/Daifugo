using System;
using Xunit;
using Daifugo;

namespace DaifugoTest
{
    public class DaifugoTest
    {
        [Fact]
        public void Run_Test()
        {
            var server = SingleProcessMessageTransceiver.Server;
            var daifugo = new DaifugoDealer(new DaifugoDealer.Configuration(5, null), server);

        }
    }
}
