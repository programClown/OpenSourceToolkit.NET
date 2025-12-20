using System;
using System.Numerics;

namespace OpenSourceToolkit.Converters
{
    public static class EthConverter
    {
        // 1 ETH = 10^18 Wei
        // 1 Gwei = 10^9 Wei

        public static decimal ToWei(decimal eth)
        {
            return eth * 1_000_000_000_000_000_000m;
        }

        public static decimal ToGwei(decimal eth)
        {
            return eth * 1_000_000_000m;
        }

        public static decimal FromWei(decimal wei)
        {
            return wei / 1_000_000_000_000_000_000m;
        }

        public static decimal FromGwei(decimal gwei)
        {
            return gwei / 1_000_000_000m;
        }
    }
}
