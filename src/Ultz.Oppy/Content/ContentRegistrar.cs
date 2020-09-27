// This file is part of Oppy.
// 
// You may modify and distribute Oppy under the terms
// of the MIT license. See the LICENSE file for details.
// 

using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ultz.Oppy.Content.Html;
using Ultz.Oppy.Content.Injection;
using Ultz.Oppy.Core;
using Ultz.Oppy.Properties;
using Ultz.Oppy.Scripting;

namespace Ultz.Oppy.Content
{
    /// <summary>
    /// The registrar for a host's content. Responsible for facilitating the registration of handlers, files, and
    /// serving their content to the web.
    /// </summary>
    public class ContentRegistrar : IDisposable
    {
        private readonly ILogger<ContentRegistrar>? _logger;
        private readonly FileSystemWatcher _wwwWatcher;

        private ConcurrentDictionary<string, RegisteredContent> _currentFileSystem =
            new ConcurrentDictionary<string, RegisteredContent>();

        private Func<HttpContext, Task>? _handleAsyncAggregated;
        private Func<string, string, Task>? _loadFileAsyncAggregated;

        /// <summary>
        /// Creates a registrar from the given content directory for the given host.
        /// </summary>
        /// <param name="wwwDir">The content directory.</param>
        /// <param name="parent">The host to which this registrar belongs.</param>
        public ContentRegistrar(string wwwDir, Host parent)
        {
            _logger = parent.Parent.LoggerFactory?.CreateLogger<ContentRegistrar>();
            Parent = parent;
            WwwDir = new DirectoryInfo(wwwDir);

            Handlers.CollectionChanged += OnHandlersChanged;
            Handlers.Add(new ScriptHandler());
            Handlers.Add(new FileHandler());
            _wwwWatcher = new FileSystemWatcher(WwwDir.FullName) {IncludeSubdirectories = true};
            _wwwWatcher.Changed += HandleChange;
            _wwwWatcher.Created += HandleChange;
            _wwwWatcher.Deleted += HandleChange;
            _wwwWatcher.Renamed += HandleChange;
        }

        /// <summary>
        /// The host to which this registrar belongs.
        /// </summary>
        public Host Parent { get; }

        /// <summary>
        /// The directory in which the host's content resides.
        /// </summary>
        public DirectoryInfo WwwDir { get; }

        /// <summary>
        /// The handlers registered to this registrar.
        /// </summary>
        public ObservableCollection<IHandler> Handlers { get; } = new ObservableCollection<IHandler>();

        /// <summary>
        /// The path to an error page template. If null, the one embedded in Oppy will be used.
        /// </summary>
        public string? ErrorPage { get; } = null;

        /// <inheritdoc />
        public void Dispose()
        {
            _wwwWatcher.Dispose();
        }

        internal void Activate()
        {
            // TODO bind to the error event
            _wwwWatcher.EnableRaisingEvents = true;
            ReloadFileSystem();
        }

        private void OnHandlersChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Handlers.Aggregate(out _handleAsyncAggregated, out _loadFileAsyncAggregated, NoHandlerAsync);
            foreach (var newItem in e.NewItems)
            {
                if (newItem is null)
                {
                    continue;
                }

                _logger.LogInformation(LogMessages.Registering, newItem.GetType());
                Needle.Inject(newItem, Parent.Parent, Parent);
            }
        }

        private void HandleChange(object src, FileSystemEventArgs args)
        {
            ReloadFileSystemAsync();
        }

        private Task ReloadFileSystemAsync()
        {
            return Task.Run(ReloadFileSystem);
        }

        private void ReloadFileSystem()
        {
            var oldFileSystem = _currentFileSystem;
            var newFileSystem = WwwDir.GetFiles("*", SearchOption.AllDirectories)
                .ToConcurrentDictionary(x => x.FullName, RegisteredContent.CreateFrom);
            Parallel.ForEach(newFileSystem.Concat(oldFileSystem).Distinct(), async file =>
            {
                var oppyPath = file.Key.FilePathToOppyPath(WwwDir, false);
                if (!oldFileSystem.ContainsKey(file.Key) && newFileSystem.ContainsKey(file.Key))
                {
                    // file added
                    await LoadFileAsync(oppyPath, file.Key);
                }
                else if (oldFileSystem.Contains(file) && !newFileSystem.Contains(file))
                {
                    // file deleted
                    await LoadFileAsync(oppyPath, file.Key);
                }
                else if (oldFileSystem[file.Key] != newFileSystem[file.Key])
                {
                    // file modified
                    await LoadFileAsync(oppyPath, file.Key);
                }
            });

            Interlocked.Exchange(ref _currentFileSystem, newFileSystem);
        }

        private async Task LoadFileAsync(string? oppyPath, string diskPath)
        {
            if (!(oppyPath is null))
            {
                var sw = Stopwatch.StartNew();
                _logger.LogInformation(LogMessages.LoadingFile, diskPath, oppyPath);
                await (_loadFileAsyncAggregated?.Invoke(oppyPath, diskPath) ?? Task.CompletedTask);
                _logger.LogInformation(LogMessages.LoadedFile, diskPath, oppyPath, sw.Elapsed.TotalMilliseconds);
            }
        }

        /// <summary>
        /// Handles the given <see cref="HttpContext" /> using the registered handlers.
        /// </summary>
        /// <param name="ctx">The context to handle.</param>
        /// <returns>An asynchronous task.</returns>
        public async Task HandleAsync(HttpContext ctx)
        {
            await Task.Run(async () =>
            {
                try
                {
                    await (_handleAsyncAggregated?.Invoke(ctx) ?? NoHandlerAsync(ctx));
                }
                catch (Exception ex1)
                {
                    try
                    {
                        ctx.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                        await WriteErrorAsync(HttpStatusCode.InternalServerError, ctx, ex1.ToString());
                    }
                    catch (Exception ex2)
                    {
                        ex1 = new AggregateException(ex1, ex2);
                    }

                    _logger.LogError(LogMessages.UnhandledException, ex1);
                }
            }).ConfigureAwait(false);
        }

        private async Task NoHandlerAsync(HttpContext ctx)
        {
            ctx.Response.StatusCode = 404;
            await WriteErrorAsync(HttpStatusCode.NotFound, ctx);
        }

        private async Task WriteErrorAsync(HttpStatusCode code, HttpContext ctx, string? longError = null)
        {
            await ctx.Response.WriteAsync(HtmlMixins.ErrorPageMixin(
                ErrorPage is null ? Properties.ErrorPage.Content : await File.ReadAllTextAsync(ErrorPage), code,
                longError));
        }
    }
}