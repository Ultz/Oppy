using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Ultz.Oppy.Content.Injection;
using Ultz.Oppy.Core;

namespace Ultz.Oppy.Content
{
    public class ContentRegistrar : IDisposable
    {
        public Host Parent { get; }
        private readonly DirectoryInfo _wwwDir;
        private readonly FileSystemWatcher _wwwWatcher;
        private Func<HttpContext, Task>? _handleAsyncAggregated;
        private Func<string, string, Task>? _loadFileAsyncAggregated;
        public ObservableCollection<IHandler> Handlers { get; } = new ObservableCollection<IHandler>();
        public ContentRegistrar(string wwwDir, Host parent)
        {
            Parent = parent;
            _wwwDir = new DirectoryInfo(wwwDir);
            
            Handlers.CollectionChanged += OnHandlersChanged;
            Handlers.Add(new FileHandler(_wwwDir));
            _wwwWatcher = new FileSystemWatcher(_wwwDir.FullName) {IncludeSubdirectories = true};
            _wwwWatcher.Changed += HandleChange;
            _wwwWatcher.Created += HandleChange;
            _wwwWatcher.Deleted += HandleChange;
            _wwwWatcher.Renamed += HandleRename;
            // TODO bind to the error event
            _wwwWatcher.EnableRaisingEvents = true;
            foreach (var file in Directory.GetFiles(wwwDir, "*", SearchOption.AllDirectories))
            {
                LoadFile(file.FilePathToOppyPath(_wwwDir), file);
            }
        }

        private void OnHandlersChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Handlers.Aggregate(out _handleAsyncAggregated, out _loadFileAsyncAggregated);
            foreach (var newItem in e.NewItems)
            {
                Needle.Inject(newItem, Parent.Parent, Parent);
            }
        }

        private void HandleRename(object src, RenamedEventArgs args)
        {
            LoadFile(args.OldFullPath.FilePathToOppyPath(_wwwDir, false), args.OldFullPath); // handlers should be configured to remove invalid entries they may hold
            LoadFile(args.FullPath.FilePathToOppyPath(_wwwDir), args.FullPath);
        }

        private void HandleChange(object src, FileSystemEventArgs args)
            => LoadFile(args.FullPath.FilePathToOppyPath(_wwwDir), args.FullPath);

        private void LoadFile(string? oppyPath, string diskPath)
        {
            if (!(oppyPath is null))
            {
                _loadFileAsyncAggregated?.Invoke(oppyPath, diskPath);
            }
        }

        /// <inheritdoc />
        public void Dispose() => _wwwWatcher.Dispose();

        public async Task HandleAsync(HttpContext ctx) => await (_handleAsyncAggregated?.Invoke(ctx) ?? NoHandler(ctx));

        public Task NoHandler(HttpContext ctx) => Task.CompletedTask; // TODO 404
    }
}