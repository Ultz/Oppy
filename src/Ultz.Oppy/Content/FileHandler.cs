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
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Ultz.Oppy.Content.Injection;
using Ultz.Oppy.Core;

namespace Ultz.Oppy.Content
{
    /// <summary>
    /// Your default, every-day, file to HTTP handler. Supports caching.
    /// </summary>
    public class FileHandler : IHandler
    {
        private readonly ConcurrentDictionary<string, string> _files = new ConcurrentDictionary<string, string>();

        private readonly ConcurrentDictionary<string, MemoryMappedFileInfo> _memoryMappedFiles =
            new ConcurrentDictionary<string, MemoryMappedFileInfo>();

        /// <summary>
        /// Gets or sets the maximum file size of a cached file.
        /// </summary>
        [OppyConfig("file.maxCachePerFile")]
        public uint MaxCachePerFile { get; set; } = 1024 * 1024;

        /// <inheritdoc />
        public async Task LoadFileAsync(string oppyPath, string diskPath, Func<Task> next)
        {
            if (!File.Exists(diskPath))
            {
                if (_memoryMappedFiles.TryGetValue(oppyPath, out var memoryMappedFile))
                {
                    memoryMappedFile.Dispose();
                    _memoryMappedFiles.TryRemove(oppyPath, out _);
                }

                _files.TryRemove(oppyPath, out _);
                await next();
                return;
            }

            var fileInfo = new FileInfo(diskPath);
            _files.AddOrUpdate(oppyPath, diskPath, (_, __) => diskPath);
            if (fileInfo.Length <= MaxCachePerFile)
            {
                try
                {
                    var memoryMappedFile = new MemoryMappedFileInfo(new FileInfo(diskPath));
                    _memoryMappedFiles.AddOrUpdate(oppyPath, memoryMappedFile, (_, old) =>
                    {
                        old.Dispose();
                        return memoryMappedFile;
                    });
                }
                catch
                {
                    // do nothing, we just won't cache
                }
            }
        }

        /// <inheritdoc />
        public async Task HandleAsync(HttpContext ctx, Func<Task> next)
        {
            var oppyPath = ctx.GetOppyPath();
            string? file = null;
            if (_files.TryGetValue(oppyPath, out var val))
            {
                file = val;
            }

            var indexFile = _files.Select(x => (KeyValuePair<string, string>?) x)
                .FirstOrDefault(x => x.Value.Key.StartsWith(oppyPath + "/index") &&
                                     !x.Value.Key.Substring(oppyPath.Length + 6).Contains('/') &&
                                     File.Exists(x.Value.Value));
            file ??= indexFile?.Value;
            if (!(file is null))
            {
                var fileInfo = _memoryMappedFiles.TryGetValue(file, out var memoryMappedFileInfo)
                    ? (IFileInfo) memoryMappedFileInfo
                    : new PhysicalFileInfo(new FileInfo(file));
                await ctx.Response.SendFileAsync(fileInfo);
            }
            else
            {
                await next();
            }
        }
    }
}