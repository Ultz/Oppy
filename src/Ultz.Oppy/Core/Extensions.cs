using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ultz.Extensions.PrivacyEnhancedMail;

namespace Ultz.Oppy.Core
{
    public static class Extensions
    {
        public static IWebHostBuilder UseOppy(this IWebHostBuilder builder, Action<Listener> configure)
        {
            var listener = new Listener();
            configure(listener);
            var coalescedInfo = new Dictionary<ushort, List<Host>>();
            Coalesce(listener, coalescedInfo);
            return builder.ConfigureServices(services =>
                    services.Add(new ServiceDescriptor(typeof(Listener), _ => listener, ServiceLifetime.Singleton)))
                .UseKestrel((ctx, opts) =>
                {
                    if (ctx.HostingEnvironment.IsDevelopment())
                    {
                    }

                    foreach (var kvp in coalescedInfo)
                    {
                        opts.ListenAnyIP(kvp.Key, listenerOpts =>
                        {
                            listenerOpts.UseConnectionLogging();
                            if (kvp.Value.Any(x => x.Listeners[kvp.Key].Item1 != SslProtocols.None))
                            {
                                listenerOpts.UseHttps(httpsOpts =>
                                {
                                    httpsOpts.SslProtocols = kvp.Value.Select(x => x.Listeners[kvp.Key].Item1)
                                        .Aggregate((x, y) => x | y);
                                    httpsOpts.ServerCertificateSelector = (context, s) =>
                                    {
                                        foreach (var host in kvp.Value)
                                        {
                                            if (host.Listeners[kvp.Key].Item3.TryGetValue(s.ToLower(), out var val))
                                            {
                                                return val;
                                            }
                                        }

                                        return null;
                                    };
                                });
                            }
                        });
                    }
                });
        }

        public static string? FilePathToOppyPath(this string path, DirectoryInfo wwwDir, bool mustExist = true)
            => new FileInfo(path).FileSystemInfoToOppyPath(wwwDir, mustExist);

        public static string? FileSystemInfoToOppyPath(this FileSystemInfo info, DirectoryInfo wwwDir, bool mustExist = true)
        {
            if (mustExist && !info.Exists)
            {
                return null;
            }

            if (info is FileInfo fileInfo &&
                !(fileInfo.Directory is null || !fileInfo.Directory.FullName.StartsWith(wwwDir.FullName)) ||
                info is DirectoryInfo directoryInfo &&
                !(directoryInfo.Parent is null || !directoryInfo.Parent.FullName.StartsWith(wwwDir.FullName)))
            {
                return Path.GetRelativePath(info.FullName, wwwDir.FullName)
                    .Replace(Path.VolumeSeparatorChar, '/')
                    .Replace(Path.DirectorySeparatorChar, '/')
                    .Replace(Path.AltDirectorySeparatorChar, '/')
                    .TrimEnd('/')
                    .ToLower();
            }

            return null;
        }

        public static string GetOppyPath(this HttpContext ctx) => ctx.Request.Path.Value.TrimEnd('/').ToLower();

        public static void RunOppy(this IApplicationBuilder builder)
            => builder.Use(_ => builder.ApplicationServices.GetService<Listener>().HandleAsync);

        private static void Coalesce(Listener listener, Dictionary<ushort, List<Host>> result)
        {
            foreach (var host in listener.Hosts)
            {
                foreach (var info in host.Listeners)
                {
                    if (!result.TryGetValue(info.Key, out var val))
                    {
                        val = result[info.Key] = new List<Host>();
                    }

                    val.Add(host);
                }
            }
        }
    }
}