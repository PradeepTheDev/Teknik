using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Web;
using Teknik.Configuration;
using Teknik.Hubs;
using static Teknik.Hubs.ServerUsageHub;

namespace Teknik.SignalR
{
    public class ServerUsageTicker
    {
        // Singleton instance
        private readonly static Lazy<ServerUsageTicker> _instance = new Lazy<ServerUsageTicker>(() => new ServerUsageTicker(GlobalHost.ConnectionManager.GetHubContext<ServerUsageHub>().Clients));

        private readonly ServerUsage _serverUsage = new ServerUsage();

        private readonly object _updateServerUsageLock = new object();

        private readonly double _updateInterval = 100;

        private readonly System.Timers.Timer _timer;
        private volatile bool _updatingServerUpdates = false;

        public ServerUsageTicker(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;

            _timer = new System.Timers.Timer(_updateInterval);
            _timer.Elapsed += UpdateServerUsage;
            _timer.AutoReset = false;
            _timer.Enabled = true;
        }

        public static ServerUsageTicker Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        private IHubConnectionContext<dynamic> Clients
        {
            get;
            set;
        }

        private void UpdateServerUsage(object source, ElapsedEventArgs e)
        {
            lock (_updateServerUsageLock)
            {
                if (!_updatingServerUpdates)
                {
                    _updatingServerUpdates = true;

                    // Update Server Usage
                    if (TryUpdateServerUsage())
                    {
                        BroadcastServerUsage(_serverUsage);
                    }

                    _updatingServerUpdates = false;

                    // Restart the timer
                    _timer.Enabled = true;
                }
            }
        }

        private void BroadcastServerUsage(ServerUsage serverUsage)
        {
            Clients.All.updateServerUsage(serverUsage);
        }

        private bool TryUpdateServerUsage()
        {
            try
            {
                Config config = Config.Load();

                // CPU
                PerformanceCounter totalCPU = new PerformanceCounter();
                // Memory
                PerformanceCounter totalAvailMem = new PerformanceCounter();
                PerformanceCounter webMem = new PerformanceCounter();
                PerformanceCounter dbMem = new PerformanceCounter();
                // Network
                PerformanceCounter sentPerf = new PerformanceCounter();
                PerformanceCounter receivedPerf = new PerformanceCounter();

                string processName = Process.GetCurrentProcess().ProcessName;

                // CPU
                totalCPU = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);

                // Memory
                totalAvailMem = new PerformanceCounter("Memory", "Available Bytes", true);
                webMem = new PerformanceCounter("Process", "Private Bytes", processName, true);
                if (config.StatsConfig.ShowDatabaseStatus)
                {
                    dbMem = new PerformanceCounter("Process", "Private Bytes", config.StatsConfig.DatabaseProcessName, true);
                }
                // Network
                if (config.StatsConfig.ShowNetworkStatus)
                {
                    sentPerf = new PerformanceCounter("Network Interface", "Bytes Sent/sec", config.StatsConfig.NetworkInterface, true);
                    receivedPerf = new PerformanceCounter("Network Interface", "Bytes Received/sec", config.StatsConfig.NetworkInterface, true);
                }

                // CPU Sample
                totalCPU.NextValue();

                // Network Sample
                sentPerf.NextValue();
                receivedPerf.NextValue();

                // Wait the sample time
                Thread.Sleep(1000);

                // CPU Values
                _serverUsage.CPU.Total = totalCPU.NextValue();

                // Memory Values
                _serverUsage.Memory.Total = config.StatsConfig.TotalMemory;
                _serverUsage.Memory.Available = totalAvailMem.NextValue();
                _serverUsage.Memory.Used = _serverUsage.Memory.Total - _serverUsage.Memory.Available;
                if (config.StatsConfig.ShowWebStatus)
                {
                    _serverUsage.Memory.WebsiteUsed = webMem.NextValue();
                }
                if (config.StatsConfig.ShowDatabaseStatus)
                {
                    _serverUsage.Memory.DatabaseUsed = dbMem.NextValue();
                }

                // Network Values
                if (config.StatsConfig.ShowNetworkStatus)
                {
                    _serverUsage.Network.Sent = sentPerf.NextValue() * 8;
                    _serverUsage.Network.Received = receivedPerf.NextValue() * 8;
                }
                return true;
            }
            catch (Exception ex)
            {
                Logging.Logger.WriteEntry(ex);
            }
            return false;
        }
    }
}
