using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ultz.Oppy.Configuration
{
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
    }
}