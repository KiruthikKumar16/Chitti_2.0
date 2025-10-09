using System;
using System.Management;
using System.Net.NetworkInformation;

namespace LineBuddy.Services
{
    public class SystemMonitorService
    {
        public SystemInfo GetSystemInfo()
        {
            return new SystemInfo
            {
                CurrentTime = DateTime.Now.ToString("HH:mm"),
                BatteryStatus = GetBatteryStatus(),
                NetworkStatus = GetNetworkStatus()
            };
        }

        private string GetBatteryStatus()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery"))
                {
                    foreach (ManagementObject battery in searcher.Get())
                    {
                        var estimatedChargeRemaining = Convert.ToInt32(battery["EstimatedChargeRemaining"]);
                        var batteryStatus = Convert.ToUInt16(battery["BatteryStatus"]);
                        
                        string icon = batteryStatus == 2 ? "🔌" : "🔋"; // 2 = charging
                        return $"{icon} {estimatedChargeRemaining}%";
                    }
                }
            }
            catch
            {
                // Fallback for systems without battery (desktops)
            }
            
            return "🔌 AC";
        }

        private string GetNetworkStatus()
        {
            try
            {
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                    {
                        if (ni.OperationalStatus == OperationalStatus.Up && 
                            ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                        {
                            if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                            {
                                return "📶 WiFi";
                            }
                            else if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                            {
                                return "🌐 Ethernet";
                            }
                        }
                    }
                }
            }
            catch
            {
                // Handle errors gracefully
            }
            
            return "❌ No Network";
        }
    }

    public class SystemInfo
    {
        public string CurrentTime { get; set; }
        public string BatteryStatus { get; set; }
        public string NetworkStatus { get; set; }
    }
}