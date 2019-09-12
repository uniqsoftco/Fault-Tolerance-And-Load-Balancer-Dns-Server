using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;

namespace Fault_Tolerance_Dns_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Global.Setup_Config();
            Parse_Args(args);
            MyPerformanceCounter.Start_Counter();
            MyDnsServer.Start_Dns_Server();
            MyPingSort.Start_PingSort();
            




            /*DnsClient d = new DnsClient(IPAddress.Parse("88.218.16.116"), 2000);
            while (true)
            {
                Console.ReadLine();
                var dm = d.Resolve(DomainName.Parse("network.uniqsoft.ir"));
                foreach (var v in dm.AnswerRecords)
                {
                    var a = v as ARecord;
                    Console.WriteLine(a.Address);
                }
            }*/




            Console.ReadLine();
        }

        static void Parse_Args(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg.Contains("-ccf"))
                {
                    //create or replace config.json file.
                    Global.Serialize_And_Save_Config_Json();
                }
            }
        }
    }
}
