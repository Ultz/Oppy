using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Ultz.Extensions.PrivacyEnhancedMail;
using Ultz.Oppy.Configuration;
using Ultz.Oppy.Content;

namespace Ultz.Oppy.Core
{
    public class Host
    {
        public Listener Parent { get; }

        public Host(in HostInfo info, Listener parent)
        {
            Parent = parent;
            Listeners = new Dictionary<ushort, (SslProtocols, HttpProtocols, Dictionary<string, X509Certificate2>)>();
            foreach (var listener in info.Listeners)
            {
                var ssl = SslProtocols.None;
                var protos = HttpProtocols.None;
                foreach (var protocol in listener.Protocols ?? Enumerable.Empty<string>())
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
                            Console.WriteLine("TLS 1.3 not supported. Reverting to TLS 1.2...");
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
                        listener.KeyPairs.Select(x => (x.Key.Trim().ToLower(), x.Value))
                            .Where(x => !(x.Value.PemCert is null) && !(x.Value.PemKey is null))
                            .ToDictionary(x => x.Item1, x => Pem.GetCertificate(x.Value.PemCert, x.Value.PemKey))));
            }

            Names = info.ServerNames?.Select(x => x.Trim().ToLower()).ToArray();
            Content = new ContentRegistrar(info.ContentDirectory, this);
            Config = info.Config;
        }

        public ContentRegistrar Content { get; }
        public string[]? Names { get; }
        public JsonElement Config { get; }

        public Dictionary<ushort, (SslProtocols, HttpProtocols, Dictionary<string, X509Certificate2>)> Listeners
        {
            get;
        }
    }
}