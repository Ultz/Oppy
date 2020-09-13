using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ultz.Oppy.Content
{
    public class ContentRegistrar : IDisposable
    {
        private readonly string _wwwDir;
        private readonly FileSystemWatcher _wwwWatcher;

        public ContentRegistrar(string wwwDir)
        {
            _wwwDir = wwwDir;
            _wwwWatcher = new FileSystemWatcher(_wwwDir);
            _wwwWatcher.Changed += HandleChange;
            _wwwWatcher.Created += HandleChange;
            _wwwWatcher.Deleted += HandleChange;
            _wwwWatcher.Renamed += HandleRename;
            // TODO bind to the error event
            _wwwWatcher.EnableRaisingEvents = true;
            foreach (var fileSystemEntry in Directory.GetFileSystemEntries(wwwDir))
            {
                HandleChange(this,
                    new FileSystemEventArgs(WatcherChangeTypes.Created, wwwDir, Path.GetFileName(fileSystemEntry)));
            }
        }

        public void HandleChange(object src, FileSystemEventArgs args)
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (args.ChangeType)
            {
                case WatcherChangeTypes.Created:
                {
                    
                    break;
                }
                case WatcherChangeTypes.Deleted:
                {
                    break;
                }
                case WatcherChangeTypes.Changed:
                {
                    break;
                }
            }
        }

        private void HandleRename(object src, RenamedEventArgs args)
        {
            HandleChange(this,
                new FileSystemEventArgs(WatcherChangeTypes.Deleted, Path.GetDirectoryName(args.OldFullPath)!,
                    args.OldName));
            HandleChange(this,
                new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetDirectoryName(args.FullPath)!, args.Name));
        }

        public void Dispose()
        {
            _wwwWatcher.Dispose();
        }

        public Task HandleAsync(HttpContext context)
        {
            throw new NotImplementedException();
        }
    }
}