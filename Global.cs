using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Text;
using ARSoft.Tools.Net;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1;

namespace Fault_Tolerance_Dns_Server
{
    public class MyConfig
    {
        public string DnsServer { get; set; }
        public string FaultToleranceDomain { get; set; }
        public int FaultToleranceDomainTtl_s { get; set; }
        public string ServersDomain { get; set; }
        public string ServerUsageFakeDomain { get; set; }
        public int ServerUsageFakeDomainTtl_s { get; set; }
        public bool PrintDebug { get; set; }
    }
    public static class Global
    {
        public static MyConfig Config { get; set; }
        private static readonly MyConfig DefaultConfig = new MyConfig
        {
            DnsServer = "8.8.8.8",
            FaultToleranceDomain = "network.uniqsoft.ir",
            FaultToleranceDomainTtl_s = 5,
            ServersDomain = "servers.uniqsoft.ir",
            ServerUsageFakeDomain = "usage.ta98",
            ServerUsageFakeDomainTtl_s = 1,
            PrintDebug = true
        };
        public static string ConfigJsonPath { get; set; } =
            Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", String.Empty))
            + "/config.json";
        public static void Setup_Config()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ConfigJsonPath = "/" + ConfigJsonPath;
            }
            /*Console.WriteLine(Global.ConfigJsonPath);
            Console.WriteLine(File.Exists(ConfigJsonPath));*/
            if (File.Exists(ConfigJsonPath))
            {
                Deserialize_And_Set_Config_Json();
            }
            else
            {
                Config = DefaultConfig;
            }
        }

        public static void Deserialize_And_Set_Config_Json()
        {
            try
            {
                string strJson = File.ReadAllText(ConfigJsonPath);
                //Console.WriteLine(strJson);
                Config = JsonConvert.DeserializeObject<MyConfig>(strJson);

                Check_Config_And_Make_It_Valid();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Environment.Exit(-1);
            }
        }

        public static void Check_Config_And_Make_It_Valid()
        {
            IPAddress tmpIpAddress;
            //Console.WriteLine(Config.DnsServer);
            if (!IPAddress.TryParse(Config.DnsServer, out tmpIpAddress))
            {
                Config.DnsServer = DefaultConfig.DnsServer;
            }

            DomainName tmpDomainName;
            if (!DomainName.TryParse(Config.FaultToleranceDomain, out tmpDomainName))
            {
                Config.FaultToleranceDomain = DefaultConfig.FaultToleranceDomain;
            }

            if (!DomainName.TryParse(Config.ServerUsageFakeDomain, out tmpDomainName))
            {
                Config.ServerUsageFakeDomain = DefaultConfig.ServerUsageFakeDomain;
            }

            if (!DomainName.TryParse(Config.ServersDomain, out tmpDomainName))
            {
                Config.ServersDomain = DefaultConfig.ServersDomain;
            }
        }
        public static void Serialize_And_Save_Config_Json()
        {
            string strJson = JsonConvert.SerializeObject(Config, Formatting.Indented);
            File.WriteAllText(ConfigJsonPath, strJson);
        }
        public static void Print(string text)
        {
            if (!Config.PrintDebug)
                return;

            if (text.EndsWith("\r\n") || text.EndsWith("\n\r"))
                text = text.Remove(text.Length - 2, 2);
            else if (text.EndsWith("\r") || text.EndsWith("\n"))
                text = text.Remove(text.Length - 1, 1);

            Console.WriteLine("------------------------------------------------------------------\r\n"
                              + text + "\r\n------------------------------------------------------------------");
        }
    }
}
