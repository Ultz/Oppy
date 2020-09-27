// This file is part of Oppy.
// 
// You may modify and distribute Oppy under the terms
// of the MIT license. See the LICENSE file for details.

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ultz.Oppy.Configuration
{
    /// <summary>
    /// Encapsulates JSON installation configuration options. These options are used to point Oppy to where the rest of
    /// the files are located.
    /// </summary>
    public struct InstallationInfo
    {
        /// <summary>
        /// The default built-in file name in which this JSON configuration is contained.
        /// </summary>
        /// <remarks>
        /// Relative to the <see cref="AppContext.BaseDirectory" />.
        /// </remarks>
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
        public readonly InstallationInfo GetAbsolute(string file)
        {
            return new InstallationInfo
            {
                HostsDir = Path.GetFullPath(HostsDir, Path.GetDirectoryName(file)!),
                PluginsDir = Path.GetFullPath(PluginsDir, Path.GetDirectoryName(file)!)
            };
        }

        /// <summary>
        /// Gets the contents of the default built-in JSON installation configuration file.
        /// </summary>
        /// <returns>The JSON installation configuration.</returns>
        public static InstallationInfo Get()
        {
            return JsonSerializer
                .Deserialize<InstallationInfo>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, FileName)))
                .GetAbsolute(Path.Combine(AppContext.BaseDirectory, FileName));
        }
    }
}