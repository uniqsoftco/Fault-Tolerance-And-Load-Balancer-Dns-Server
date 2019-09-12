using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;

namespace Fault_Tolerance_Dns_Server
{
    public static class MyDnsClient
    {
        private static readonly DnsClient DnsClient = new DnsClient(IPAddress.Parse(Global.Config.DnsServer), 2000);

        public static List<IPAddress> Get_IP_Address_List(string domain)
        {
            DnsMessage answer = DnsClient.Resolve(DomainName.Parse(domain));
            List<IPAddress> ipAddresses = new List<IPAddress>();

            if (answer != null)
                foreach (DnsRecordBase dnsRecord in answer.AnswerRecords)
                {
                    if (dnsRecord is ARecord aRecord)
                    {
                        ipAddresses.Add(aRecord.Address);
                    }
                }

            return ipAddresses;
        }

        public static int Get_Server_Usage_Percent_If_Alive(IPAddress ip)
        {
            DnsClient dnsClient = new DnsClient(ip, 1500);

            DnsMessage answer = dnsClient.Resolve(DomainName.Parse(Global.Config.ServerUsageFakeDomain));


            if (answer != null)
                foreach (var additionalRecord in answer.AdditionalRecords)
                {
                    if (additionalRecord is ARecord aRecord &&
                        aRecord.Name.IsEqualOrSubDomainOf(DomainName.Parse(Global.Config.ServerUsageFakeDomain)))
                    {
                        return Convert.ToInt32(aRecord.Name.Labels[0]);
                    }
                }



            return 505;
        }
    }
}
