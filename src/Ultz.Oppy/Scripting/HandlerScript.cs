// This file is part of Oppy.
// 
// You may modify and distribute Oppy under the terms
// of the MIT license. See the LICENSE file for details.
// 

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Ultz.Oppy.Core;

namespace Ultz.Oppy.Scripting
{
    /// <summary>
    /// Represents a scripted handler, routed to by the <see cref="ScriptHandler" />
    /// </summary>
    public class HandlerScript
    {
        /// <summary>
        /// Creates a scripted handler using the given handler delegate and path matching mode (for the default path
        /// matcher)
        /// </summary>
        /// <param name="handler">The handler delegate to use.</param>
        /// <param name="pathMatchingMode">The path matching mode, used by the default path matcher.</param>
        public HandlerScript(Func<HttpContext, Func<Task>, Task> handler, PathMatchingMode pathMatchingMode)
            : this(handler, GetFor(pathMatchingMode))
        {
        }

        /// <summary>
        /// Creates a scripted handler using the given handler delegate and path matcher.
        /// </summary>
        /// <param name="handler">The handler delegate to use.</param>
        /// <param name="pathMatcher">The path matcher to use.</param>
        public HandlerScript(Func<HttpContext, Func<Task>, Task> handler,
            Func<FileInfo, FileSystemInfo, bool> pathMatcher)
        {
            Handler = handler;
            PathMatcher = pathMatcher;
        }

        /// <summary>
        /// The handler delegate to use.
        /// </summary>
        public Func<HttpContext, Func<Task>, Task> Handler { get; }

        /// <summary>
        /// A delegate which determines whether a script should be applied to a path.
        /// </summary>
        /// <remarks>
        /// The delegate arguments are as follows:
        /// <list type="bullet">
        /// <item>
        /// <term>Argument 1 (Script Path)</term>
        /// <description>The path to the CSX script file in which the handler is contained.</description>
        /// </item>
        /// <item>
        /// <term>Argument 2 (Path To Compare)</term><description>The path to attempt to match to this script.</description>
        /// </item>
        /// </list>
        /// </remarks>
        public Func<FileInfo, FileSystemInfo, bool> PathMatcher { get; } // scriptPath, pathToCompare, isMatch

        /// <summary>
        /// Gets the <see cref="FileInfo" /> representing the path to the script on disk. Generally isn't null, but may be
        /// if attempting to be fetched after instantiation but before registration with the <see cref="ScriptHandler" />
        /// </summary>
        public FileInfo? ScriptDiskInfo { get; internal set; }

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
                   fileSystemInfo.Exists &&
                   fileSystemInfo is FileInfo fileInfo
                    // if in the same directory and the file matches, or if in a subdirectory and it matches
                    ? fileInfo.Directory?.FullName != script.Directory?.FullName
                        ? dirMatcher?.Invoke(script, fileInfo.Directory!) ?? false
                        : fileMatcher?.Invoke(script, fileInfo) ?? false
                    : fileSystemInfo is DirectoryInfo directoryInfo &&
                      (dirMatcher?.Invoke(script, directoryInfo) ?? false);

            static bool AnyDirMatcher(FileInfo script, DirectoryInfo dir)
            {
                return dir.FullName.StartsWith(script.FullName[..^4]);
            }

            static bool ExactDirMatcher(FileInfo script, DirectoryInfo dir)
            {
                return dir.FullName.Equals(script.FullName[..^4]);
            }

            static bool AnyFileMatcher(FileInfo script, FileInfo fileInfo)
            {
                return Path.GetFileNameWithoutExtension(fileInfo.FullName) ==
                       Path.GetFileNameWithoutExtension(script.FullName) ||
                       ExactFileMatcher(script, fileInfo);
            }

            static bool ExactFileMatcher(FileInfo script, FileInfo fileInfo)
            {
                return Path.GetFileName(fileInfo.FullName) ==
                       Path.GetFileNameWithoutExtension(script.FullName);
            }
        }

        /// <summary>
        /// Determines whether this script is applicable to this disk path by running it through the <see cref="PathMatcher" />.
        /// </summary>
        /// <param name="diskPath"></param>
        /// <returns></returns>
        public bool DiskPathMatches(string diskPath)
        {
            return PathMatcher.Invoke(ScriptDiskInfo ?? throw new InvalidOperationException("ScriptDiskInfo not set."),
                Directory.Exists(diskPath)
                    ? (FileSystemInfo) new DirectoryInfo(diskPath)
                    : new FileInfo(diskPath));
        }

        internal void AddAllMatches(ConcurrentDictionary<string, HandlerScript> scripts, Host host)
        {
            if (ScriptDiskInfo is null)
            {
                throw new InvalidOperationException("ScriptDiskInfo not set.");
            }

            AddMatches(ScriptDiskInfo.Directory?.FullName ??
                       throw new NullReferenceException("Script's directory not found."));

            void AddMatches(string directory)
            {
                foreach (var file in Directory.GetFiles(directory))
                {
                    var path = directory.DirectoryPathToOppyPath(host.Content.WwwDir);
                    if (Path.GetFileName(file).ToLower().StartsWith("index") && !(path is null))
                    {
                        scripts.AddOrUpdate(path, this, (_, __) => this);
                    }

                    path = file.FilePathToOppyPath(host.Content.WwwDir);
                    if (DiskPathMatches(file) && !(path is null))
                    {
                        scripts.AddOrUpdate(path, this, (_, __) => this);
                    }

                    path = Path.Combine(ScriptDiskInfo.Directory.FullName, Path.GetFileNameWithoutExtension(file))
                        .FilePathToOppyPath(host.Content.WwwDir, false);
                    if (file == ScriptDiskInfo.FullName && !(path is null))
                    {
                        scripts.AddOrUpdate(path, this, (_, __) => this);
                    }
                }

                foreach (var subdirectory in Directory.GetDirectories(directory, "*"))
                {
                    var path = subdirectory.DirectoryPathToOppyPath(host.Content.WwwDir);
                    if (DiskPathMatches(subdirectory) && !(path is null))
                    {
                        scripts.AddOrUpdate(path, this, (_, __) => this);
                        AddMatches(subdirectory);
                    }
                }
            }
        }
    }
}