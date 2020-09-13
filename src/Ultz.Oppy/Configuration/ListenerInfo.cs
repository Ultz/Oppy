using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ultz.Oppy.Configuration
{
    public struct ListenerInfo
    {
        [JsonPropertyName("proto")]
        public string[]? Protocols { get; set; }
        [JsonPropertyName("port")]
        public ushort Port { get; set; }
        [JsonPropertyName("keyPairs")]
        public Dictionary<string, PemKeyPair> KeyPairs { get; set; }
    }

    public struct PemKeyPair
    {
        [JsonPropertyName("certPem")]
        public string? PemCert { get; set; }
        [JsonPropertyName("keyPem")]
        public string? PemKey { get; set; }
    }
}