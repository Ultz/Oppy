// This file is part of Oppy.
// 
// You may modify and distribute Oppy under the terms
// of the MIT license. See the LICENSE file for details.
// 

using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ultz.Oppy.Configuration
{
    /// <summary>
    /// Encapsulates JSON host configuration options.
    /// </summary>
    public struct HostInfo
    {
        /// <summary>
        /// Gets or sets the root directory of the hosts content. Relative to the host info file.
        /// </summary>
        [JsonPropertyName("wwwDir")]
        public string ContentDirectory { get; set; }

        /// <summary>
        /// Gets or sets an array of server names.
        /// </summary>
        [JsonPropertyName("names")]
        public string[]? ServerNames { get; set; }

        /// <summary>
        /// Gets or sets an array of listener configurations.
        /// These are coalesced by Oppy on boot and
        /// </summary>
        [JsonPropertyName("listen")]
        public ListenerInfo[] Listeners { get; set; }

        /// <summary>
        /// Miscellaneous configuration, used throughout Oppy and its handlers.
        /// </summary>
        [JsonPropertyName("config")]
        public JsonElement Config { get; set; }

        /// <summary>
        /// The name of this host. Doesn't affect HTTP operations at all, and is primarily for logging & recognition
        /// purposes.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Returns an instance with each path converted to an absolute file.
        /// </summary>
        public readonly HostInfo GetAbsolute(string filePath)
        {
            var info = this;
            info.ContentDirectory = Path.GetFullPath(ContentDirectory, Path.GetDirectoryName(filePath)!);
            info.Name ??= Path.GetFileNameWithoutExtension(filePath);
            info.Listeners = info.Listeners.Select(x =>
            {
                var listenerInfo = x;
                listenerInfo.KeyPairs = listenerInfo.KeyPairs?.ToDictionary(y => y.Key,
                    y => new PemKeyPair
                    {
                        PemCert = y.Value.PemCert is null
                            ? null
                            : Path.GetFullPath(y.Value.PemCert, Path.GetDirectoryName(filePath)!),
                        PemKey = y.Value.PemKey is null
                            ? null
                            : Path.GetFullPath(y.Value.PemKey, Path.GetDirectoryName(filePath)!)
                    });
                return listenerInfo;
            }).ToArray();
            return info;
        }
    }
}