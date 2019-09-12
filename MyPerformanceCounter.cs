using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Org.BouncyCastle.Asn1.Mozilla;

namespace Fault_Tolerance_Dns_Server
{
    public static class MyPerformanceCounter
    {
        private static float _cpuUsage;
        private static readonly object CpuUsage_lock = new object();
        private static readonly int PerformanceCounterInterval = 1000;
        public static float CpuUsage
        {
            get
            {
                lock (CpuUsage_lock)
                    return _cpuUsage;
            }
            private set
            {
                lock (CpuUsage_lock)
                    _cpuUsage = (_cpuUsage + value) / 2;
            }
        }
        public static void Start_Counter()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Start_Counter_Windows();
            else
                Start_Counter_Linux();
        }

        private static void Start_Counter_Windows()
        {
            PerformanceCounter cpuCounter = new PerformanceCounter
            {
                CategoryName = "Processor",
                CounterName = "% Processor Time",
                InstanceName = "_Total"
            };
            Global.Print("Performance Counter Windows Started.");

            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(PerformanceCounterInterval);
                    CpuUsage = cpuCounter.NextValue();
                    //Console.WriteLine(CpuUsage);
                }
            });
        }
        private static void Start_Counter_Linux()
        {

            Process vmstatProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"echo $[100-$(vmstat 1 2|tail -1|awk '{{print $15}}')]\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            Global.Print("Performance Counter Linux Started.");

            Task.Run(() =>
            {
                while (true)
                {
                    vmstatProcess.Start();
                    vmstatProcess.WaitForExit(2000);
                    CpuUsage = Convert.ToSingle(vmstatProcess.StandardOutput.ReadLine());
                    //Console.WriteLine(CpuUsage);
                }
            });

        }
    }
}
