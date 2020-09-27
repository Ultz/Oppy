// This file is part of Oppy.
// 
// You may modify and distribute Oppy under the terms
// of the MIT license. See the LICENSE file for details.
// 

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;
using Ultz.Oppy.Content;
using Ultz.Oppy.Content.Injection;
using Ultz.Oppy.Core;
using Ultz.Oppy.Properties;

namespace Ultz.Oppy.Scripting
{
    /// <summary>
    /// A handler which is responsible for facilitating the use of C# script files (CSX files) and routing
    /// <see cref="HttpContext" />s to them as it sees fit.
    /// </summary>
    public class ScriptHandler : IHandler
    {
        private static readonly string[] _imports = AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location))
            .SelectMany(x => x.GetTypes().Select(y => y.Namespace))
            .Where(x => (x?.StartsWith("System") ?? false) || (x?.StartsWith("Ultz") ?? false))
            .Distinct()
            .ToArray()!;

        private readonly ConcurrentDictionary<string, HandlerScript> _scripts =
            new ConcurrentDictionary<string, HandlerScript>();

        /// <summary>
        /// Further C# script imports to be added on top of the default System.* and Ultz.* imports. Injected from JSON config by
        /// Oppy.
        /// </summary>
        [OppyConfig("scripting.additionalCsxImports")]
        public string[] AdditionalCsxImports { get; set; } = Array.Empty<string>();

        /// <summary>
        /// The host to which this handler belongs. Injected by Oppy.
        /// </summary>
        [OppyInject]
        public Host Host { get; set; } = null!;

        /// <summary>
        /// The listener to which this handler belongs. Injected by Oppy.
        /// </summary>
        [OppyInject]
        public IListener Listener { get; set; } = null!;

        /// <summary>
        /// This handler's logger. Created and injected by Oppy.
        /// </summary>
        [OppyInject]
        public ILogger<ScriptHandler>? Logger { get; set; } = null;

        /// <inheritdoc />
        public async Task LoadFileAsync(string oppyPath, string diskPath, Func<Task> next)
        {
            var notExist = !File.Exists(diskPath);
            if (notExist)
            {
                if (_scripts.TryGetValue(oppyPath, out var script))
                {
                    var toRemove = new List<string>();
                    if (Path.GetExtension(diskPath) == ".csx")
                    {
                        foreach (var kvp in _scripts)
                        {
                            if (kvp.Value == script)
                            {
                                toRemove.Add(kvp.Key);
                            }
                        }
                    }
                    else
                    {
                        toRemove.Add(oppyPath);
                    }

                    foreach (var s in toRemove)
                    {
                        _scripts.TryRemove(s, out _);
                    }
                }

                await next();
                return;
            }

            if (Path.GetExtension(diskPath) != ".csx")
            {
                var toAdd = new List<(string, HandlerScript)>();
                foreach (var script in _scripts.Values)
                {
                    if (script.DiskPathMatches(diskPath))
                    {
                        toAdd.Add((oppyPath, script));
                    }
                }

                foreach (var val in toAdd)
                {
                    _scripts.AddOrUpdate(val.Item1, val.Item2, (_, __) => val.Item2);
                }

                await next();
                return;
            }

            try
            {
                var result = await CSharpScript.EvaluateAsync(await File.ReadAllTextAsync(diskPath), ScriptOptions
                    .Default.WithAllowUnsafe(true).WithImports(_imports.Concat(AdditionalCsxImports)).WithReferences(
                        AppDomain.CurrentDomain.GetAssemblies()
                            .Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location))));
                if (result is HandlerScript handlerScript)
                {
                    // do nothing more
                }
                else if (result is ValueTuple<IHandler, PathMatchingMode> handlerTuple1)
                {
                    Needle.Inject(handlerTuple1.Item1, Listener, Host);
                    handlerScript = new HandlerScript(handlerTuple1.Item1.HandleAsync, handlerTuple1.Item2);
                }
                else if (result is ValueTuple<PathMatchingMode, IHandler> handlerTuple2)
                {
                    Needle.Inject(handlerTuple2.Item2, Listener, Host);
                    handlerScript = new HandlerScript(handlerTuple2.Item2.HandleAsync, handlerTuple2.Item1);
                }
                else if (result is IHandler handler)
                {
                    Needle.Inject(handler, Listener, Host);
                    handlerScript = new HandlerScript(handler.HandleAsync, PathMatchingMode.Default);
                }
                else if (result is NoMixinEncapsulation noMixinHandler)
                {
                    Host.Content.Handlers.Insert(0, noMixinHandler.Handler);
                    return;
                }
                else
                {
                    // not a valid script. don't await next for security reasons 
                    return;
                }

                handlerScript.ScriptDiskInfo = new FileInfo(diskPath);
                handlerScript.AddAllMatches(_scripts, Host);
            }
            catch (Exception e)
            {
                Logger?.LogError(LogMessages.ScriptEvalError, oppyPath, e);
                // not a valid script. don't await next for security reasons 
            }
        }

        /// <inheritdoc />
        public async Task HandleAsync(HttpContext ctx, Func<Task> next)
        {
            if (_scripts.TryGetValue(ctx.GetOppyPath(), out var script))
            {
                await script.Handler(ctx, next);
            }
            else
            {
                await next();
            }
        }

        private class GlobalEncapsulation // TODO
        {
            private readonly ScriptHandler _handler;

            public GlobalEncapsulation(ScriptHandler handler)
            {
                _handler = handler;
            }

            public IListener Listener => _handler.Listener;
            public Host Host => _handler.Host;
        }
    }
}