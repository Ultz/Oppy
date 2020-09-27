// This file is part of Oppy.
// 
// You may modify and distribute Oppy under the terms
// of the MIT license. See the LICENSE file for details.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ultz.Oppy.Configuration
{
    /// <summary>
    /// Encapsulates JSON listener configuration.
    /// </summary>
    public struct ListenerInfo
    {
        /// <summary>
        /// Gets or sets an array of protocols used by this listener.
        /// </summary>
        [JsonPropertyName("proto")]
        public string[]? Protocols { get; set; }

        /// <summary>
        /// Gets or sets the port to listen on.
        /// </summary>
        [JsonPropertyName("port")]
        public ushort Port { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of key pairs where the key denotes the server name the key pair is applicable for,
        /// and the value denotes the location of the PEM certificate and key for this listener.
        /// </summary>
        /// <remarks>
        /// Only used if one of the protocols is TLS.
        /// </remarks>
        [JsonPropertyName("keyPairs")]
        public Dictionary<string, PemKeyPair>? KeyPairs { get; set; }
    }

    /// <summary>
    /// Encapsulates the location of a PEM key pair (i.e. certificate/public key and a private key)
    /// </summary>
    public struct PemKeyPair
    {
        /// <summary>
        /// The location of the PEM certificate/public key relative to this file.
        /// </summary>
        [JsonPropertyName("certPem")]
        public string? PemCert { get; set; }

        /// <summary>
        /// The location of the PEM private key relative to this file.
        /// </summary>
        [JsonPropertyName("keyPem")]
        public string? PemKey { get; set; }
    }
}