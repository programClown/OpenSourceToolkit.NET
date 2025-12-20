using DnsClient;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace OpenSourceToolkit.Networking
{
    public class DnsLookupTool
    {
        private readonly LookupClient _lookupClient;

        public DnsLookupTool(IPAddress dnsServer = null)
        {
            _lookupClient = dnsServer != null ? new LookupClient(dnsServer) : new LookupClient();
        }

        public async Task<List<string>> QueryAsync(string domain, QueryType queryType)
        {
            var result = await _lookupClient.QueryAsync(domain, queryType);
            var answers = new List<string>();

            foreach (var answer in result.Answers)
            {
                answers.Add(answer.ToString());
            }

            return answers;
        }

        public async Task<Dictionary<string, List<string>>> BatchQueryAsync(string domain)
        {
            var types = new[] { QueryType.A, QueryType.AAAA, QueryType.MX, QueryType.TXT, QueryType.CNAME, QueryType.NS };
            var results = new ConcurrentDictionary<string, List<string>>();

            var tasks = types.Select(async type =>
            {
                try
                {
                    var lookup = await QueryAsync(domain, type);
                    if (lookup.Count > 0)
                    {
                        results[type.ToString()] = lookup;
                    }
                }
                catch
                {
                    // Ignore failures for specific record types
                }
            });

            await Task.WhenAll(tasks);

            return results.ToDictionary(k => k.Key, v => v.Value);
        }
    }
}
