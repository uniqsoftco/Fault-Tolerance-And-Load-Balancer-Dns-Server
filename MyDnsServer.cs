using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;

namespace Fault_Tolerance_Dns_Server
{
    public static class MyDnsServer
    {
        private static readonly DnsServer DnsServer = new DnsServer(10, 10);
        public static void Start_Dns_Server()
        {
            Global.Print("Dns Server Started.");
            DnsServer.QueryReceived += DnsServer_QueryReceived;

            DnsServer.Start();
        }

        private static Task DnsServer_QueryReceived(object sender, QueryReceivedEventArgs eventArgs)
        {
            Global.Print("Query Received.");
            return Task.Run(() =>
            {
                if (!(eventArgs.Query is DnsMessage query))
                {
                    return;
                }

                /*var query = eventArgs.Query as DnsMessage;

                Global.Print("AnswerRecords");
                foreach (var record in query.AnswerRecords)
                {
                    Console.WriteLine(record.ToString());
                }
                Global.Print("AuthorityRecords");
                foreach (var record in query.AuthorityRecords)
                {
                    Console.WriteLine(record.ToString());
                }
                Global.Print("Questions");
                foreach (var record in query.Questions)
                {
                    Console.WriteLine(record.ToString());
                }
                Global.Print("AdditionalRecords");
                foreach (var record in query.AdditionalRecords)
                {
                    Console.WriteLine(record.ToString());
                }
                return;*/

                DnsQuestion question = query.Questions[0];
                DnsMessage response = query.CreateResponseInstance();

                Global.Print($"Query: {question.Name}?");
                if (question.Name.IsEqualOrSubDomainOf(DomainName.Parse(Global.Config.FaultToleranceDomain)))
                {

                    var firstStillServer = MyPingSort.Get_First_Still_Server();
                    response.AnswerRecords.Add(
                        new ARecord(
                            DomainName.Parse(Global.Config.FaultToleranceDomain),
                            Global.Config.FaultToleranceDomainTtl_s,
                            firstStillServer
                        ));
                    Global.Print($"Answer: {firstStillServer}");

                    response.ReturnCode = ReturnCode.NoError;
                }
                else if (question.Name.IsEqualOrSubDomainOf(DomainName.Parse(Global.Config.ServerUsageFakeDomain)))
                {
                    try
                    {
                        string strAnswer = Convert.ToInt32(MyPerformanceCounter.CpuUsage).ToString() + ".usage.ta98";
                        response.AdditionalRecords.Add(
                            new ARecord(
                                DomainName.Parse(strAnswer),
                                Global.Config.ServerUsageFakeDomainTtl_s,
                                IPAddress.Any
                            ));
                        Global.Print($"Answer: {strAnswer}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
                else
                {

                    Global.Print("Answer: NotZone");
                    response.ReturnCode = ReturnCode.NotZone;
                }

                eventArgs.Response = response;
            });
        }
    }
}
