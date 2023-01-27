﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using CZGL.SystemInfo;


namespace ServerStatus_CSharpClient
{
    internal class main
    {
        static TcpClient Client = new();
        static string ServerIP,Username,Password;
        static int Port;
        static double Interval = 2;
        static Platform OSPlatform;
        enum Platform
        {
            Windows,
            Linux,
        }
        
        public struct ServerStatus
        {
            public long Uptime;//在线时间
            public double Load;//负载（仅Linux）
            public long MemTotal;
            public long MemUsed;
            public long SwapTotal;//交换大小
            public long SwapUsed;//已使用交换
            public long DiskTotal;//储存大小
            public long DiskUsed;//已使用储存
            public double CPU;//使用百分比
            public long NetIn;//入速率
            public long NetOut;//出速率
            public long NetTx;//出流量
            public long NetRx;//入流量
        }
        static void Main(string[] args)
        {
            if (RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                OSPlatform = Platform.Windows;
            else
                OSPlatform = Platform.Linux;
            if (args.Length != 0)
            {
                string arg = null;
                foreach (string _arg in args)
                {
                    if (_arg.Contains("dsn"))
                    {
                        arg = _arg.Replace("-", "");
                        continue;
                    }
                    else if (_arg.Contains("interval"))
                    {
                        arg = _arg.Replace("-", "");
                        continue;
                    }
                    else if (arg == "dsn")
                    {
                        string[] InputArg = _arg.Replace("\"","").Replace("\"", "").Split(':', '@');
                        Username = InputArg[0];
                        Password = InputArg[1];
                        ServerIP = InputArg[2];
                        Port = int.Parse(InputArg[3]);
                    }
                    else if (arg == "interval")
                        Interval = double.Parse(_arg);
                    else
                    {
                        Console.WriteLine($"[ERROR]Invalid argument \"{_arg}\"");
                        Environment.Exit(0);
                    }

                }
            }
            else
            {
                Console.WriteLine($"[ERROR]None argument");
                Environment.Exit(0);
            }
            Connect();
        }
        public static void Connect()
        {
            string data = "";
            while(!Client.Connected)
            {
                Console.WriteLine("[INFO]Trying to connect...");
                try
                {
                    Client.Connect(ServerIP, Port);
                }
                catch
                {
                    Console.WriteLine("[INFO]Connect Failed");
                }
                Thread.Sleep(1000);
            }
            Socket ClientSocket = Client.Client;
            data = Receive(ClientSocket);
            Console.WriteLine(data);
            while (!data.Contains("Authentication successful"))
            {                
                Send($"{Username}:{Password}\n", ClientSocket);
                data = Receive(ClientSocket);
                Console.WriteLine(data);
                Thread.Sleep(1000);
            }
            data = Receive(ClientSocket);
            Console.WriteLine(data);
            Console.WriteLine("[INFO]Connection Succeeded");
            Update(ClientSocket);
        }
        public static string Receive(Socket _Socket)
        {
            byte[] Buffer = new byte[1024];
            _Socket.Receive(Buffer,0,1024,SocketFlags.None);
            return Encoding.UTF8.GetString(Buffer);
        }
        public static void Send(string Text, Socket _Socket)
        {
            try
            {
                var data = Encoding.UTF8.GetBytes(Text);
                _Socket.Send(data, 0, data.Length, SocketFlags.None);
            }
            catch
            {
                if(!_Socket.Connected)
                    Connect();
            }
        }
        public static ServerStatus? GetStatus()
        {
            ServerStatus status = new();
            //通用
            status.Uptime = (long)TimeSpan.FromMilliseconds(Environment.TickCount).TotalSeconds;
            //CPU
            CPUTime CPURunTime = CPUHelper.GetCPUTime();
            Thread.Sleep((int)Interval * 50);
            var _CPURunTime = CPUHelper.GetCPUTime();
            status.CPU = (double)decimal.Round((decimal)(CPUHelper.CalculateCPULoad(CPURunTime, _CPURunTime) * 100),0);            
            //Mem
            var Memory = MemoryHelper.GetMemoryValue();
            status.MemTotal = (long)Memory.TotalPhysicalMemory / 1000;
            status.MemUsed = (long)Memory.UsedPhysicalMemory / 1000;
            //Swap
            status.SwapTotal = (long)Memory.TotalVirtualMemory / 10000000;
            status.SwapUsed = (long)Memory.UsedVirtualMemory / 10000000;
            //NIC Info
            var NICs = NetworkInterface.GetAllNetworkInterfaces();
            long NICIn = 0,NICOut = 0;
            foreach(var NIC in NICs)
            {
                NICOut += NIC.GetIPStatistics().BytesSent;
                NICIn += NIC.GetIPStatistics().BytesReceived;
            }
            //速率计算
            List<long> InBytes = new();
            List<long> OutBytes = new();
            for (int count = 0;count < 10;count++)
            {
                long TotalInBytes = 0;
                long TotalOutBytes = 0;
                foreach (var NIC in NICs)
                {
                    //if (NIC.OperationalStatus != OperationalStatus.Up || NIC.NetworkInterfaceType == NetworkInterfaceType.Loopback || NIC.GetIPStatistics().BytesSent == 0 || NIC.GetIPStatistics().BytesReceived == 0)
                    //    continue;
                    TotalOutBytes += NIC.GetIPStatistics().BytesSent;
                    TotalInBytes += NIC.GetIPStatistics().BytesReceived;
                }
                InBytes.Add(TotalInBytes);
                OutBytes.Add(TotalOutBytes);
                Thread.Sleep((int)(Interval *50));
            }
            double InSpeed = 0,OutSpeed = 0;
            for(int index = 0;index < InBytes.Count - 1; index++)
            {
                InSpeed += (InBytes[index + 1] - InBytes[index]) / (Interval * 50 / 1000);
                OutSpeed += (OutBytes[index + 1] - OutBytes[index]) / (Interval * 50 / 1000);
            }
            InSpeed = InSpeed / 10;
            OutSpeed = OutSpeed / 10;
            //Disk Info
            var Disklist = DriveInfo.GetDrives();
            long StorageTotalSpace = 0;
            long StorageFreeSpace = 0;
            long StorageUsedSpace = 0;
            foreach (var Disk in Disklist)
            {
                if (Disk.DriveType != DriveType.Fixed)
                    continue;
                StorageTotalSpace += Disk.TotalSize;
                StorageFreeSpace += Disk.TotalFreeSpace;
            }
            StorageFreeSpace = StorageFreeSpace / 1000000;
            StorageTotalSpace = StorageTotalSpace / 1000000;
            StorageUsedSpace = StorageTotalSpace - StorageFreeSpace;
            //
            status.NetIn = NICIn;
            status.NetOut = NICOut;
            status.NetRx = (long)InSpeed;
            status.NetTx = (long)OutSpeed;
            status.DiskTotal = StorageTotalSpace;
            status.DiskUsed = StorageUsedSpace;
            //CPUHandle.Wait();
            //status.CPU = 100.0;
            

            return status;
        }
        public static void Update(Socket Socket)
        {
            while(true)
            {
                Console.WriteLine("\n[INFO]Getting Status...");
                var Status = (ServerStatus)GetStatus();
                string Text = $"update {{\"uptime\":{Status.Uptime},\"load\":{Status.Load},\"memory_total\":{Status.MemTotal},\"memory_used\":{Status.MemUsed},\"swap_total\":{Status.SwapTotal},\"swap_used\":{Status.SwapUsed},\"hdd_total\":{Status.DiskTotal},\"hdd_used\":{Status.DiskUsed},\"cpu\":{Status.CPU}.0,\"network_tx\":{Status.NetTx},\"network_rx\":{Status.NetRx},\"network_in\":{Status.NetIn},\"network_out\":{Status.NetOut},\"online4\":false,\"online6\":false}}\n";
                Console.WriteLine($"Uptime:{Status.Uptime}");
                Console.WriteLine($"Load:{Status.Load}");
                Console.WriteLine($"MemTotal:{Status.MemTotal}");
                Console.WriteLine($"MemUsed:{Status.MemUsed}");
                Console.WriteLine($"SwpTotal:{Status.SwapTotal}");
                Console.WriteLine($"SwpUsed:{Status.SwapUsed}");
                Console.WriteLine($"DiskTotal:{Status.DiskTotal}");
                Console.WriteLine($"DiskUsed:{Status.DiskUsed}");
                Console.WriteLine($"CPU:{Status.CPU}");
                Console.WriteLine($"NetTx:{Status.NetTx}");
                Console.WriteLine($"NetRx:{Status.NetRx}");
                Console.WriteLine($"NetIn:{Status.NetIn}");
                Console.WriteLine($"NetOut:{Status.NetOut}");
                Send(Text,Socket);
                Thread.Sleep((int)(Interval * 1000));
            }
        }
    }
}
