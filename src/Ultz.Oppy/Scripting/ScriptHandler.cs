using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Ultz.Oppy.Content;
using Ultz.Oppy.Core;

namespace Ultz.Oppy.Scripting
{
    public class ScriptHandler : IHandler
    {
        private readonly Dictionary<string, HandlerScript> _scripts = new Dictionary<string, HandlerScript>();
        public async Task LoadFileAsync(string oppyPath, string diskPath, Func<Task> next)
        {
            if (!File.Exists(diskPath))
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
                        _scripts.Remove(s);
                    }
                }

                return;
            }

            if (Path.GetExtension(diskPath) != ".csx")
            {
                foreach (var script in _scripts.Values)
                {
                    if (script.DiskPathMatches(diskPath))
                    {
                        _scripts.Add(oppyPath, script);
                        break;
                    }
                }

                await next();
                return;
            }

            try
            {
                var result = await CSharpScript.EvaluateAsync(oppyPath);
                if (result is HandlerScript handlerScript)
                {
                    // do nothing more
                }
                else if (result is ValueTuple<IHandler, PathMatchingMode> handlerTuple1)
                {
                    handlerScript = new HandlerScript(handlerTuple1.Item1.HandleAsync, handlerTuple1.Item2);
                }
                else if (result is ValueTuple<PathMatchingMode, IHandler> handlerTuple2)
                {
                    handlerScript = new HandlerScript(handlerTuple2.Item2.HandleAsync, handlerTuple2.Item1);
                }
                else if (result is IHandler handler)
                {
                    handlerScript = new HandlerScript(handler.HandleAsync, PathMatchingMode.Default);
                }
                else
                {
                    // not a valid script. don't await next for security reasons 
                    return;
                }

                handlerScript.ScriptDiskInfo = new FileInfo(diskPath);
                _scripts.Add(oppyPath, handlerScript);
                handlerScript.AddAllMatches(_scripts);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error when evaluating script at \"{oppyPath}\", it will no longer be served: {e}");
                // not a valid script. don't await next for security reasons 
            }
        }

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
    }
}