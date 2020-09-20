using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ultz.Oppy.Configuration
{
    public struct InstallationInfo
    {
        public const string FileName = "installationinfo.json";

        /// <summary>
        /// Hosts dir. Usually relative to the installation info file, which is in the same directory as Ultz.Oppy.dll.
        /// </summary>
        [JsonPropertyName("hosts")]
        public string HostsDir { get; set; }

        /// <summary>
        /// Plugins dir. Usually relative to the installation info file, which is in the same directory as Ultz.Oppy.dll.
        /// </summary>
        [JsonPropertyName("plugins")]
        public string PluginsDir { get; set; }

        /// <summary>
        /// Dependencies dir (DLLs that aren't plugins).
        /// Usually relative to the installation info file, which is in the same directory as Ultz.Oppy.dll.
        /// </summary>
        [JsonPropertyName("deps")]
        public string DependenciesDir { get; set; }

        /// <summary>
        /// Gets a copy of this struct with absolute paths, relative to the JSON file passed in.
        /// </summary>
        /// <param name="file">The absolute installationinfo.json path.</param>
        /// <returns></returns>
        public readonly InstallationInfo GetAbsolute(string file) => new InstallationInfo
        {
            HostsDir = Path.GetFullPath(HostsDir, Path.GetDirectoryName(file)!),
            PluginsDir = Path.GetFullPath(PluginsDir, Path.GetDirectoryName(file)!),
        };

        public static InstallationInfo Get()
            => JsonSerializer
                .Deserialize<InstallationInfo>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, FileName)))
                .GetAbsolute(Path.Combine(AppContext.BaseDirectory, FileName));
    }
}