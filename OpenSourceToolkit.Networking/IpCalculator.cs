using System;
using System.Net;

namespace OpenSourceToolkit.Networking
{
    public static class IpCalculator
    {
        public static (string NetworkAddress, string BroadcastAddress, long NumberOfHosts) Calculate(string ipAddress, string subnetMask)
        {
            // This is a simplified implementation
            if (!IPAddress.TryParse(ipAddress, out var ip) || !IPAddress.TryParse(subnetMask, out var mask))
            {
                throw new ArgumentException("Invalid IP or Subnet Mask");
            }

            byte[] ipBytes = ip.GetAddressBytes();
            byte[] maskBytes = mask.GetAddressBytes();

            if (ipBytes.Length != maskBytes.Length)
                throw new ArgumentException("IP and Mask address families mismatch");

            byte[] networkBytes = new byte[ipBytes.Length];
            byte[] broadcastBytes = new byte[ipBytes.Length];

            for (int i = 0; i < ipBytes.Length; i++)
            {
                networkBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
                broadcastBytes[i] = (byte)(networkBytes[i] | ~maskBytes[i]);
            }

            var network = new IPAddress(networkBytes).ToString();
            var broadcast = new IPAddress(broadcastBytes).ToString();

            // Calculate hosts count (simplified for IPv4, assuming contiguous mask)
            // Only accurate for IPv4 standard masks
            long hosts = 0;
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                uint maskVal = (uint)((maskBytes[0] << 24) | (maskBytes[1] << 16) | (maskBytes[2] << 8) | maskBytes[3]);
                uint inverted = ~maskVal;
                hosts = inverted - 1;
            }

            return (network, broadcast, hosts > 0 ? hosts : 0);
        }

        public static string CidrToMask(int cidr)
        {
            if (cidr < 0 || cidr > 32) throw new ArgumentOutOfRangeException(nameof(cidr));
            uint mask = 0xffffffff << (32 - cidr);
            byte[] bytes = new byte[]
            {
                (byte)((mask >> 24) & 0xff),
                (byte)((mask >> 16) & 0xff),
                (byte)((mask >> 8) & 0xff),
                (byte)(mask & 0xff)
            };
            return new IPAddress(bytes).ToString();
        }
    }
}
