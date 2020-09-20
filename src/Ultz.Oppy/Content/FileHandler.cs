using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ultz.Oppy.Content
{
    public class FileHandler : IHandler
    {
        private readonly DirectoryInfo _wwwDir;

        private readonly Dictionary<string, MemoryMappedFile> _memoryMappedFiles =
            new Dictionary<string, MemoryMappedFile>();

        private readonly Dictionary<string, string> _files = new Dictionary<string, string>();

        public FileHandler(DirectoryInfo wwwDir)
        {
            _wwwDir = wwwDir;
        }

        public async Task LoadFileAsync(string oppyPath, string diskPath, Func<Task> next)
        {
            if (!File.Exists(diskPath))
            {
                if (_memoryMappedFiles.TryGetValue(oppyPath, out var memoryMappedFile))
                {
                    memoryMappedFile.Dispose();
                    _memoryMappedFiles.Remove(oppyPath);
                }

                _files.Remove(oppyPath);
                return;
            }
        }

        public Task HandleAsync(HttpContext ctx, Func<Task> next)
        {
            throw new NotImplementedException();
        }
    }
}