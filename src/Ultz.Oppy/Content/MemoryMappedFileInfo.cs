// This file is part of Oppy.
// 
// You may modify and distribute Oppy under the terms
// of the MIT license. See the LICENSE file for details.

using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using Microsoft.Extensions.FileProviders;

namespace Ultz.Oppy.Content
{
    internal class MemoryMappedFileInfo : IFileInfo, IDisposable
    {
        private readonly FileInfo _fileInfo;
        private readonly MemoryMappedFile _memoryMappedFile;

        public MemoryMappedFileInfo(FileInfo fileInfo)
        {
            _fileInfo = fileInfo;
            _memoryMappedFile = MemoryMappedFile.CreateNew(
                PhysicalPath.Replace(Path.PathSeparator, '_').Replace(Path.DirectorySeparatorChar, '_')
                    .Replace(Path.AltDirectorySeparatorChar, '_').Replace(Path.VolumeSeparatorChar, '_'), Length);
            using var file = fileInfo.OpenRead();
            using var view = _memoryMappedFile.CreateViewStream(0, Length, MemoryMappedFileAccess.Write);
            file.CopyTo(view);
            view.Flush();
        }

        public void Dispose()
        {
            _memoryMappedFile.Dispose();
        }

        public Stream CreateReadStream()
        {
            return _memoryMappedFile.CreateViewStream();
        }

        public bool Exists => _fileInfo.Exists;
        public long Length => _fileInfo.Length;
        public string PhysicalPath => _fileInfo.FullName;
        public string Name => _fileInfo.Name;
        public DateTimeOffset LastModified => _fileInfo.LastWriteTime;
        public bool IsDirectory => false;
    }
}