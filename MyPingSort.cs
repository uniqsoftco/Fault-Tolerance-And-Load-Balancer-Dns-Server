using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fault_Tolerance_Dns_Server
{
    public class ServerItem : IComparable<ServerItem>
    {
        public IPAddress IpAddress { get; set; }
        public int UsageByPercent { get; set; }

        public int CompareTo(ServerItem other)
        {
            if (this.UsageByPercent < other.UsageByPercent)
                return -1;

            if (this.UsageByPercent > other.UsageByPercent)
                return 1;

            return 0;
        }
    }
    public static class MyPingSort
    {
        private static readonly List<ServerItem> ServerItems = new List<ServerItem>();
        private static readonly object ServerItems_lock = new object();
        private static bool _pingSortSwitch = true;
        public static void Start_PingSort()
        {
            Task.Run(() =>
            {
                List<ServerItem> tmpServerItems;

                while (_pingSortSwitch)
                {
                    tmpServerItems = new List<ServerItem>();

                    foreach (var serverIp in MyDnsClient.Get_IP_Address_List(Global.Config.ServersDomain))
                    {
                        tmpServerItems.Add(new ServerItem
                        {
                            IpAddress = serverIp,
                            UsageByPercent = MyDnsClient.Get_Server_Usage_Percent_If_Alive(serverIp)
                        });
                    }

                    lock (ServerItems_lock)
                    {
                        ServerItems.Clear();
                        ServerItems.AddRange(tmpServerItems);
                        ServerItems.Sort();
                    }

                    string text = "";
                    tmpServerItems.Sort();
                    foreach (var t in tmpServerItems)
                        text += $"{t.IpAddress} : {t.UsageByPercent}%\r\n";
                    Global.Print(text);

                    Thread.Sleep(5000);
                }
            });
        }

        public static void Stop_PingSort()
        {
            _pingSortSwitch = false;
        }

        public static IPAddress Get_First_Still_Server()
        {
            lock (ServerItems_lock)
                if (ServerItems.Count > 0 && ServerItems[0].UsageByPercent <= 100)
                    return ServerItems[0].IpAddress;

            return IPAddress.Any;
        }
    }
}
