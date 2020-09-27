// This file is part of Oppy.
// 
// You may modify and distribute Oppy under the terms
// of the MIT license. See the LICENSE file for details.
// 

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Ultz.Oppy.Application;
using Ultz.Oppy.Properties;

namespace Ultz.Oppy.Core
{
    /// <summary>
    /// Contains common and miscellaneous Oppy extenison methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Registers Oppy to the given <see cref="IWebHostBuilder" />.
        /// </summary>
        /// <param name="builder">The web host builder to register Oppy to.</param>
        /// <param name="configure">A delegate responsible for configuring Oppy's <see cref="IListener" /> object.</param>
        /// <returns>The instance passed in, for method chaining.</returns>
        public static IWebHostBuilder UseOppy(this IWebHostBuilder builder, Action<IListener> configure)
        {
            return builder.ConfigureServices(services =>
                    services.TryAddEnumerable(ServiceDescriptor.Singleton<IListener, Listener>()))
                .UseKestrel((ctx, opts) =>
                {
                    var sw = Stopwatch.StartNew();
                    var logger = opts.ApplicationServices.GetService<ILoggerFactory>()?.CreateLogger<Startup>();
                    logger.LogInformation(LogMessages.StartingOppy);
                    var listener = opts.ApplicationServices.GetService<IListener>();
                    configure(listener);
                    var coalescedInfo = new Dictionary<ushort, List<Host>>();
                    Coalesce(listener, coalescedInfo);
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

                    logger.LogInformation(LogMessages.Started, sw.Elapsed.TotalMilliseconds);
                })
                .UseSetting(WebHostDefaults.SuppressStatusMessagesKey, "True");
        }

        /// <summary>
        /// Converts a collection to a concurrent dictionary, with keys and values selected by the given selectors.
        /// </summary>
        /// <param name="source">The collection to convert to a concurrent dictionary.</param>
        /// <param name="keySelector">The key selector, used to determine keys from collection elements.</param>
        /// <param name="elementSelector">The values selector, used to determine values from collection elements.</param>
        /// <typeparam name="TSource">The type of each element of the source collection.</typeparam>
        /// <typeparam name="TKey">The key type of the resultant dictionary.</typeparam>
        /// <typeparam name="TElement">The value type of the resultant dictionary.</typeparam>
        /// <returns>The resultant concurrent dictionary.</returns>
        public static ConcurrentDictionary<TKey, TElement> ToConcurrentDictionary<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            ConcurrentDictionary<TKey, TElement> d = new ConcurrentDictionary<TKey, TElement>();
            foreach (var element in source)
            {
                d.TryAdd(keySelector(element), elementSelector(element));
            }

            return d;
        }

        /// <summary>
        /// Converts a file path into an Oppy HTTP path.
        /// </summary>
        /// <param name="path">The file path to convert.</param>
        /// <param name="wwwDir">The content directory, used to make relative paths.</param>
        /// <param name="mustExist">Whether the file must exist. If true and the file doesn't exist, returns null.</param>
        /// <returns>The Oppy HTTP path.</returns>
        public static string? FilePathToOppyPath(this string path, DirectoryInfo wwwDir, bool mustExist = true)
        {
            return new FileInfo(path).FileSystemInfoToOppyPath(wwwDir, mustExist);
        }

        /// <summary>
        /// Converts a directory path into an Oppy HTTP path.
        /// </summary>
        /// <param name="path">The directory path to convert.</param>
        /// <param name="wwwDir">The content directory, used to make relative paths.</param>
        /// <param name="mustExist">Whether the directory must exist. If true and the directory doesn't exist, returns null.</param>
        /// <returns>The Oppy HTTP path.</returns>
        public static string? DirectoryPathToOppyPath(this string path, DirectoryInfo wwwDir, bool mustExist = true)
        {
            return new DirectoryInfo(path).FileSystemInfoToOppyPath(wwwDir, mustExist);
        }

        /// <summary>
        /// Converts a <see cref="FileSystemInfo" /> into an Oppy HTTP path.
        /// </summary>
        /// <param name="info">The <see cref="FileSystemInfo" />  to convert.</param>
        /// <param name="wwwDir">The content directory, used to make relative paths.</param>
        /// <param name="mustExist">
        /// Whether the <see cref="FileSystemInfo" /> must exist. If true and the
        /// <see cref="FileSystemInfo" /> doesn't exist, returns null.
        /// </param>
        /// <returns>The Oppy HTTP path.</returns>
        public static string? FileSystemInfoToOppyPath(this FileSystemInfo info, DirectoryInfo wwwDir,
            bool mustExist = true)
        {
            if (mustExist && !info.Exists)
            {
                return null;
            }

            if (info is FileInfo fileInfo &&
                !(fileInfo.Directory is null || !fileInfo.Directory.FullName.StartsWith(wwwDir.FullName)) ||
                info is DirectoryInfo directoryInfo &&
                directoryInfo.FullName.StartsWith(wwwDir.FullName))
            {
                var rel = Path.GetRelativePath(wwwDir.FullName, info.FullName);
                if (rel == ".")
                {
                    rel = string.Empty;
                }

                return ("/" + rel
                        .Replace(Path.VolumeSeparatorChar, '/')
                        .Replace(Path.DirectorySeparatorChar, '/')
                        .Replace(Path.AltDirectorySeparatorChar, '/'))
                    .TrimEnd('/')
                    .ToLower();
            }

            return null;
        }

        /// <summary>
        /// Gets an Oppy path representing the resource a <see cref="HttpContext" /> is requesting.
        /// </summary>
        /// <param name="ctx">The <see cref="HttpContext" /> to get an Oppy path representation of.</param>
        /// <returns>The Oppy HTTP path.</returns>
        public static string GetOppyPath(this HttpContext ctx)
        {
            return ctx.Request.Path.Value.TrimEnd('/').ToLower();
        }

        /// <summary>
        /// Adds the <see cref="Listener.HandleAsync" /> method to the <see cref="IApplicationBuilder" />'s
        /// <see cref="IServiceCollection" /> such that when the application runs, so does Oppy.
        /// </summary>
        /// <param name="builder">The application builder to run Oppy on.</param>
        public static void RunOppy(this IApplicationBuilder builder)
        {
            builder.Use(_ => builder.ApplicationServices.GetService<IListener>().HandleAsync);
        }

        private static void Coalesce(IListener listener, IDictionary<ushort, List<Host>> result)
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

        /// <summary>
        /// Adds the given hosts to an <see cref="IListener" />.
        /// </summary>
        /// <param name="this">The listener to add the hosts to.</param>
        /// <param name="hosts">The hosts to add to the listener.</param>
        /// <returns>The instance passed in, for method chaining.</returns>
        public static IListener WithHosts(this IListener @this, params Host[] hosts)
        {
            @this.Hosts.AddRange(hosts);
            return @this;
        }
    }
}