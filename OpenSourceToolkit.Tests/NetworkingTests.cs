using System.Threading.Tasks;
using DnsClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenSourceToolkit.Networking;

namespace OpenSourceToolkit.Tests
{
    [TestClass]
    public class NetworkingTests
    {
        [TestMethod]
        public async Task DummyIpGeolocationProvider_ReturnsEchoedIpAndDefaults()
        {
            IIpGeolocationProvider provider = new DummyIpGeolocationProvider();
            var result = await provider.GetLocationAsync("1.2.3.4");

            Assert.IsNotNull(result);
            Assert.AreEqual("1.2.3.4", result.Ip);
            Assert.AreEqual("Unknown", result.Country);
            Assert.AreEqual("Unknown", result.Region);
            Assert.AreEqual("Unknown", result.City);
            Assert.AreEqual("Unknown", result.Isp);
            Assert.AreEqual("UTC", result.Timezone);
        }

        [TestMethod]
        [Timeout(5000)]
        public async Task DnsLookup_Localhost_DoesNotThrowAndReturnsResultList()
        {
            var tool = new DnsLookupTool();
            // "localhost" is not a valid domain for a DNS server query (it's a hosts file entry).
            // DnsClient bypasses the OS resolver and goes straight to a DNS server.
            // We use a real domain to test the actual DNS resolution capability.
            var answers = await tool.QueryAsync("google.com", QueryType.A);

            Assert.IsNotNull(answers);
            // We expect at least one IP for google.com
            Assert.IsTrue(answers.Count > 0, "Should return at least one A record for google.com");
        }

        [TestMethod]
        [Timeout(5000)]
        public async Task DnsLookup_BatchQuery_ReturnsDictionary()
        {
            var tool = new DnsLookupTool();
            var results = await tool.BatchQueryAsync("google.com");

            Assert.IsNotNull(results);
            Assert.IsTrue(results.Count > 0);
            Assert.IsTrue(results.ContainsKey("A"));
        }

        // IpCalculator
        [TestMethod]
        public void IpCalculator_Subnet_Works()
        {
            var (network, broadcast, hosts) = IpCalculator.Calculate("192.168.1.10", "255.255.255.0");
            Assert.AreEqual("192.168.1.0", network);
            Assert.AreEqual("192.168.1.255", broadcast);
            Assert.AreEqual(254, hosts);
        }

        [TestMethod]
        public void IpCalculator_CidrToMask_Works()
        {
            string mask = IpCalculator.CidrToMask(24);
            Assert.AreEqual("255.255.255.0", mask);

            mask = IpCalculator.CidrToMask(8);
            Assert.AreEqual("255.0.0.0", mask);
        }
    }
}
