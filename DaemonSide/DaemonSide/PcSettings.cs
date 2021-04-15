using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DaemonSide
{
    class PcSettings
    {
        public string GetName()
        {
            return Environment.MachineName;
        }
        public string GetOs()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) { return "Linux"; }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { return "Windows"; }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) { return "Mac"; }
            else { return "Unknown"; }
        }
        public string GetMacAddress()
        {
            return (NetworkInterface
            .GetAllNetworkInterfaces()
            .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .Select(nic => nic.GetPhysicalAddress().ToString()))
            .FirstOrDefault();
        }
        public async Task<string> GetIpAddress()
        {
            try
            {
                Http http = new Http();
                string result = await http.GetAsync("https://ip.seeip.org");
                return result;
            }
            catch (Exception e) { }
            return "0.0.0.0";
        }
    }
}
