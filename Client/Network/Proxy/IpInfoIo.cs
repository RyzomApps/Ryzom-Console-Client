using System;
using System.Globalization;
using System.Net;
using System.Text.Json;

namespace Client.Network.Proxy
{
    /// <summary>
    /// Class to get location information about an ip address.
    /// </summary>
    internal class IpInfoIo
    {
        /// <summary>
        /// Use http://ipinfo.io to get the country information.
        /// Up to 1000 requests per day are free of charge.
        /// </summary>
        /// <param name="ip">ip address</param>
        /// <returns>ISO region code</returns>
        public static string GetUserCountryByIp(string ip)
        {
            IpInfo ipInfo;

            try
            {
                var info = new WebClient().DownloadString($"http://ipinfo.io/{ip}");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                ipInfo = JsonSerializer.Deserialize<IpInfo>(info, options);

                if (ipInfo == null)
                    return "";

                var myRi1 = new RegionInfo(ipInfo.Country);
                ipInfo.Country = myRi1.ThreeLetterISORegionName;
            }
            catch (Exception)
            {
                return "";
            }

            return ipInfo.Country;
        }

        /// <summary>
        /// class for json information response from ipinfo.io
        /// </summary>
        public class IpInfo
        {
            public string Ip { get; set; }

            public string Hostname { get; set; }

            public string City { get; set; }

            public string Region { get; set; }

            public string Country { get; set; }

            public string Loc { get; set; }

            public string Org { get; set; }

            public string Postal { get; set; }
        }
    }
}
