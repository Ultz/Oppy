// This file is part of Oppy.
// 
// You may modify and distribute Oppy under the terms
// of the MIT license. See the LICENSE file for details.
// 

using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Ultz.Oppy.Configuration;
using Ultz.Oppy.Properties;

namespace Ultz.Oppy.Core
{
    /// <summary>
    /// Contains globals specific to an Oppy standalone installation.
    /// </summary>
    public static class OppyInstallation
    {
        /// <summary>
        /// The installation info. Used to load hosts, dependencies, and plugins into Oppy.
        /// </summary>
        public static readonly InstallationInfo Info;

        /// <summary>
        /// The hosts resolved in the hosts directory, found using the <see cref="InstallationInfo" />.
        /// </summary>
        public static HostInfo[] Hosts;

        static OppyInstallation()
        {
            Info = InstallationInfo.Get();
            Hosts = LoadHosts();
        }

        private static HostInfo[] LoadHosts()
        {
            return Directory.GetFiles(Info.HostsDir, "*.json", SearchOption.AllDirectories).Select<string, HostInfo?>(
                x =>
                {
                    try
                    {
                        return JsonSerializer.Deserialize<HostInfo>(File.ReadAllText(x)).GetAbsolute(x);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(LogMessages.LoadHostFailed, e);
                        return null;
                    }
                }).Where(x => x.HasValue).Select(x => x!.Value).ToArray();
        }
    }
}