using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Ultz.Oppy.Configuration;

namespace Ultz.Oppy.Core
{
    public static class OppyInstallation
    {
        public static readonly InstallationInfo Info;
        public static HostInfo[] Hosts;

        static OppyInstallation()
        {
            Info = InstallationInfo.Get();
            Hosts = LoadHosts();
        }

        public static HostInfo[] LoadHosts()
        {
            Console.WriteLine("Loading hosts...");
            return Directory.GetFiles(Info.HostsDir, "*.json").Select<string, HostInfo?>(x =>
            {
                try
                {
                    return JsonSerializer.Deserialize<HostInfo>(File.ReadAllText(x));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Couldn't load host configuration: {e}");
                    return null;
                }
            }).Where(x => x.HasValue).Select(x => x!.Value).ToArray();
        }
    }
}