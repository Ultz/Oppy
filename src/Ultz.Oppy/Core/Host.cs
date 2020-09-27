// This file is part of Oppy.
// 
// You may modify and distribute Oppy under the terms
// of the MIT license. See the LICENSE file for details.
// 

using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Logging;
using Ultz.Extensions.PrivacyEnhancedMail;
using Ultz.Oppy.Configuration;
using Ultz.Oppy.Content;
#if !NETCOREAPP3_1
using Ultz.Oppy.Properties;

#endif

namespace Ultz.Oppy.Core
{
    /// <summary>
    /// Represents an Oppy host on one listener and, optionally, specific names. Encapsulates Oppy configuration, desired
    /// server information, and is the owner of the registrar for content. Owned by <see cref="IListener" />.
    /// </summary>
    public class Host
    {
        /// <summary>
        /// Creates a host from the given JSON configuration and parent listener.
        /// </summary>
        /// <param name="info">The JSON configuration of this host.</param>
        /// <param name="parent">The listener to which this host should belong.</param>
        public Host(in HostInfo info, IListener parent)
        {
            Names = info.ServerNames?.Select(x => x.Trim().ToLower()).ToArray();
            Config = info.Config;
            Parent = parent;
            Logger = parent.LoggerFactory?.CreateLogger<Host>();
            Name = info.Name ?? "Unnamed Host";
            Listeners = new Dictionary<ushort, (SslProtocols, HttpProtocols, Dictionary<string, X509Certificate2>)>();
            Content = new ContentRegistrar(info.ContentDirectory, this);
            foreach (var listener in info.Listeners)
            {
                var ssl = SslProtocols.None;
                var protos = HttpProtocols.None;
                foreach (var protocol in listener.Protocols ?? new[] {"http1"})
                {
                    switch (protocol.Trim().Replace(".", null).ToLower())
                    {
#if NETCOREAPP3_1
                        case "tls13":
                        {
                            ssl |= SslProtocols.Tls13;
                            break;
                        }
#else
                        case "tls13":
                        {
                            Logger?.LogWarning(LogMessages.Tls13NotSupported, Name);
                            ssl |= SslProtocols.Tls12;
                            break;
                        }
#endif
                        case "tls12":
                        {
                            ssl |= SslProtocols.Tls12;
                            break;
                        }
                        case "tls11":
                        {
                            ssl |= SslProtocols.Tls11;
                            break;
                        }
                        case "tls10":
                        {
                            ssl |= SslProtocols.Tls;
                            break;
                        }
                        case "http2":
                        {
                            protos |= HttpProtocols.Http2;
                            break;
                        }
                        case "http1":
                        {
                            protos |= HttpProtocols.Http1;
                            break;
                        }
                    }
                }

                Listeners.Add(listener.Port,
                    (ssl, protos,
                        (listener.KeyPairs ?? new Dictionary<string, PemKeyPair>())
                        .Select(x => (x.Key.Trim().ToLower(), x.Value))
                        .Where(x => !(x.Value.PemCert is null) && !(x.Value.PemKey is null))
                        .ToDictionary(x => x.Item1, x => Pem.GetCertificate(x.Value.PemCert, x.Value.PemKey))));
            }

            Content.Activate();
        }

        /// <summary>
        /// The <see cref="IListener" /> to which this <see cref="Host" /> belongs.
        /// </summary>
        public IListener Parent { get; }

        /// <summary>
        /// This host's logger.
        /// </summary>
        private ILogger<Host>? Logger { get; }

        /// <summary>
        /// The canonical name of this host.
        /// </summary>
        /// <remarks>
        /// This does not affect operation in any way, and is purely just for recognition purposes. You may be looking for
        /// <see cref="Names" />
        /// </remarks>
        /// <seealso cref="Names" />
        public string Name { get; }

        /// <summary>
        /// This host's content registrar.
        /// </summary>
        public ContentRegistrar Content { get; }

        /// <summary>
        /// The server names (domain names) on which this host's content is served. If null, no server name checks will
        /// be performed.
        /// </summary>
        public string[]? Names { get; }

        /// <summary>
        /// Miscellaneous JSON configuration options, often injected in parts into handlers and other objects used
        /// around Oppy.
        /// </summary>
        public JsonElement Config { get; }

        /// <summary>
        /// The cached desired listener information from the original <see cref="HostInfo" /> passed into the constructor. Used by
        /// <see cref="IListener" /> to route requests to the right host.
        /// </summary>
        public Dictionary<ushort, (SslProtocols, HttpProtocols, Dictionary<string, X509Certificate2>)> Listeners
        {
            get;
        }
    }
}