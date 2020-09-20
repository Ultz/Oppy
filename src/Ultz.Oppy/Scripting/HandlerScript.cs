using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ultz.Oppy.Scripting
{
    public class HandlerScript
    {
        public Func<HttpContext, Func<Task>, Task> Handler { get; }
        public Func<FileInfo, FileSystemInfo, bool> PathMatcher { get; } // scriptPath, pathToCompare, isMatch
        public FileInfo ScriptDiskInfo { get; internal set; }

        public HandlerScript(Func<HttpContext, Func<Task>, Task> handler, PathMatchingMode pathMatchingMode)
            : this(handler, GetFor(pathMatchingMode))
        {
        }

        public HandlerScript(Func<HttpContext, Func<Task>, Task> handler, Func<FileInfo, FileSystemInfo, bool> pathMatcher)
        {
            Handler = handler;
            PathMatcher = pathMatcher;
        }

        private static Func<FileInfo, FileSystemInfo, bool> GetFor(PathMatchingMode pathMatchingMode)
        {
            Func<FileInfo, FileInfo, bool>? fileMatcher = null;
            Func<FileInfo, DirectoryInfo, bool>? dirMatcher = null;
            if ((pathMatchingMode & PathMatchingMode.AnyMatchingDirOrSubDir) != 0)
            {
                dirMatcher = AnyDirMatcher;
            }
            else if ((pathMatchingMode & PathMatchingMode.ExactDirMatch) != 0)
            {
                dirMatcher = ExactDirMatcher;
            }

            if ((pathMatchingMode & PathMatchingMode.AnyMatchingFileName) != 0)
            {
                fileMatcher = AnyFileMatcher;
            }
            else if ((pathMatchingMode & PathMatchingMode.ExactFileMatch) != 0)
            {
                fileMatcher = ExactFileMatcher;
            }

            return (script, fileSystemInfo)
                => script.Exists &&
                   (!(fileSystemInfo is FileInfo file) || (fileMatcher?.Invoke(script, file) ?? false)) &&
                   (fileSystemInfo is DirectoryInfo dir && (dirMatcher?.Invoke(script, dir) ?? false) ||
                    fileSystemInfo is FileInfo f && !(f.Directory is null) &&
                    (dirMatcher?.Invoke(script, f.Directory) ?? false));

            static bool ExactDirMatcher(FileInfo script, DirectoryInfo dir)
                => dir.Exists &&
                   !(script.Directory is null) &&
                   dir.FullName
                       .TrimEnd(Path.DirectorySeparatorChar)
                       .TrimEnd(Path.AltDirectorySeparatorChar)
                       .ToLower()
                       .Equals(script.Directory.FullName
                           .TrimEnd(Path.DirectorySeparatorChar)
                           .TrimEnd(Path.AltDirectorySeparatorChar)
                           .ToLower());

            static bool AnyDirMatcher(FileInfo script, DirectoryInfo dir)
                => dir.Exists &&
                   !(script.Directory is null) &&
                   dir.FullName.TrimEnd(Path.DirectorySeparatorChar)
                       .TrimEnd(Path.AltDirectorySeparatorChar)
                       .ToLower()
                       .StartsWith(script.Directory.FullName
                           .TrimEnd(Path.DirectorySeparatorChar)
                           .TrimEnd(Path.AltDirectorySeparatorChar)
                           .ToLower()) ||
                   ExactDirMatcher(script, dir);

            static bool ExactFileMatcher(FileInfo script, FileInfo file)
                => file.Exists &&
                   Path.GetFileNameWithoutExtension(script.FullName)
                       .ToLower()
                       .Equals(Path
                           .GetFileName(file.FullName)
                           .ToLower());

            static bool AnyFileMatcher(FileInfo script, FileInfo file)
                => file.Exists &&
                   Path.GetFileNameWithoutExtension(script.FullName)
                       .ToLower()
                       .Equals(Path
                           .GetFileNameWithoutExtension(file.FullName)
                           .ToLower()) ||
                   ExactFileMatcher(script, file);
        }

        public bool DiskPathMatches(string diskPath)
            => PathMatcher.Invoke(ScriptDiskInfo, Directory.Exists(diskPath)
                ? (FileSystemInfo)new DirectoryInfo(diskPath)
                : new FileInfo(diskPath));

        public void AddAllMatches(Dictionary<string, HandlerScript> scripts)
        {
            AddMatches(ScriptDiskInfo.Directory?.FullName ??
                       throw new NullReferenceException("Script's directory not found."));
            void AddMatches(string directory)
            {
                foreach (var file in Directory.GetFiles(directory))
                {
                    if (DiskPathMatches(file))
                    {
                        scripts.Add(file, this);
                    }
                }

                foreach (var subdirectory in Directory.GetDirectories(directory))
                {
                    if (DiskPathMatches(subdirectory))
                    {
                        scripts.Add(directory, this);
                        AddMatches(subdirectory);
                    }
                }
            }
        }
    }
}